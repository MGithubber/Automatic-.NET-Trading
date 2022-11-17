using System.Data.SqlClient;

using AutomaticDotNETtrading.Application.Interfaces.Data;
using AutomaticDotNETtrading.Infrastructure.Data;
using AutomaticDotNETtrading.Infrastructure.Enums;
using AutomaticDotNETtrading.Infrastructure.Models;

using Binance.Net.Enums;
using Binance.Net.Objects.Models.Futures;

using Bogus;

using Respawn;

namespace Infrastructure.Tests.Integration.TradingDataDbServiceTests;

[TestFixture]
public class TradingDataDbServiceTests
{
    private const string ConnectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=\"Binance trading logs\";Integrated Security=True";
    private readonly IDatabaseConnectionFactory<SqlConnection> ConnectionFactory = new SqlDatabaseConnectionFactory(ConnectionString);
    private readonly ITradingDataDbService<TVCandlestick> SUT = new TradingDataDbService(ConnectionString);
    private Respawner DbRespawner = default!; // will clear all data in the database when ResetAsync is called


    #region Fakers
    private readonly Faker<TVCandlestick> CandlesticksFaker = new Faker<TVCandlestick>()
        .RuleFor(c => c.CurrencyPair, f => new CurrencyPair(f.Finance.Currency().Code, f.Finance.Currency().Code))
        .RuleFor(c => c.Date, f => f.Date.Between(f.Date.Recent(5480), f.Date.Soon(5480)))
        .RuleFor(c => c.Open, f => f.Random.Decimal(1000, 3000))
        .RuleFor(c => c.High, f => f.Random.Decimal(1000, 3000))
        .RuleFor(c => c.Low, f => f.Random.Decimal(1000, 3000))
        .RuleFor(c => c.Close, f => f.Random.Decimal(1000, 3000))
        .Rules((f, c) =>
        {
            if (f.PickRandom(true, false))
                return; // signal stays Hold

            object _ = f.PickRandom<LuxAlgoSignal>() switch
            {
                LuxAlgoSignal.Buy => c.Buy = true,
                LuxAlgoSignal.StrongBuy => c.StrongBuy = true,
                LuxAlgoSignal.Sell => c.Sell = true,
                LuxAlgoSignal.StrongSell => c.StrongSell = true,
                LuxAlgoSignal.ExitBuy => c.ExitBuy = f.Random.Double(950, 3050),
                LuxAlgoSignal.ExitSell => c.ExitSell = f.Random.Double(950, 3050),
                _ => () => { }
                ,
            };
        });

    private readonly Faker<BinanceFuturesOrder> FuturesOrdersFaker = new Faker<BinanceFuturesOrder>()
        .RuleFor(order => order.Symbol, f => f.Finance.Currency().Code)
        .RuleFor(order => order.Id, f => f.Random.Long(0, long.MaxValue))
        .RuleFor(order => order.CreateTime, f => f.Date.Between(f.Date.Recent(5480), f.Date.Soon(5480)))
        .RuleFor(order => order.Side, f => f.PickRandom<OrderSide>())
        .RuleFor(order => order.Type, f => f.PickRandom<FuturesOrderType>())
        .RuleFor(order => order.Price, f => f.Random.Decimal(1000, 3000))
        .RuleFor(order => order.Quantity, f => f.Random.Decimal(0.01m, 10));
    #endregion


    [SetUp]
    public async Task Setup()
    {
        DbRespawner = await Respawner.CreateAsync(ConnectionString);
    }

    #region Tests
    [Test, Order(1)]
    public void CreatingAndClosingConnection_Work()
    {
        // Arrange
        SqlConnection connection = default!;
        Action createConnection = new Action(() => connection = ConnectionFactory.CreateConnection());


        // Act
        // Assert
        createConnection.Should().NotThrow();

        new Action(() => connection.Open()).Should().Throw<Exception>("the connection is open already");
        new Action(() => connection.Close()).Should().NotThrow("the connection is open and it can be closed");
    }


    [Test, Order(2)]
    public void AddingCandlestick_Works()
    {
        // Arrange
        int FakeCandlestick_db_id = int.MinValue;
        TVCandlestick FakeCandlestick = CandlesticksFaker.Generate();
        Action action = new Action(() => FakeCandlestick_db_id = SUT.AddCandlestick(FakeCandlestick));

        // Act & Assert
        action.Should().NotThrow("because the candlestick is not in the database yet and can be added");
        FakeCandlestick_db_id.Should().BeGreaterThan(0);
        action.Should().Throw<Exception>("because the candlestick is in the database already and can't be added");
    }

    [Test, Order(3)]
    public void AddingFuturesOrders_Works()
    {
        // Arrange
        TVCandlestick Candlestick1 = CandlesticksFaker.Generate();
        BinanceFuturesOrder Order1 = FuturesOrdersFaker.Generate();
        Order1.CreateTime = Candlestick1.Date;
        Order1.Symbol = Candlestick1.CurrencyPair.Name;

        TVCandlestick Candlestick2 = CandlesticksFaker.Generate();
        BinanceFuturesOrder Order2 = FuturesOrdersFaker.Generate();
        Order2.CreateTime = Candlestick2.Date;
        Order2.Symbol = Candlestick2.CurrencyPair.Name;

        if (Candlestick1.Date > Candlestick2.Date)
        {
            (Candlestick1, Candlestick2) = (Candlestick2, Candlestick1);
            (Order1, Order2) = (Order2, Order1);
        }

        BinanceFuturesOrder Order3 = FuturesOrdersFaker.Generate();
        Order3.Symbol = Candlestick2.CurrencyPair.Name;


        // Act
        SUT.AddFuturesOrder(Order1, Candlestick1, out int Order1_Id, out int Candlestick1_Id);
        SUT.AddFuturesOrder(Order2, Candlestick2, out int Order2_Id, out int Candlestick2_Id);
        SUT.AddFuturesOrder(Order3, Candlestick2, out int Order3_Id, out int Candlestick3_Id);


        // Assert
        Order1_Id.Should().BeGreaterThan(0);
        Candlestick1_Id.Should().BeGreaterThan(0);
        Order2_Id.Should().BeGreaterThan(0);
        Candlestick2_Id.Should().BeGreaterThan(0);
        Order3_Id.Should().BeGreaterThan(0);
        Candlestick3_Id.Should().BeGreaterThan(0).And.Be(Candlestick2_Id);
    }


    [Test, Order(4)]
    public void DeletingCandlestick_Works()
    {
        // Arrange
        TVCandlestick FakeCandlestick = CandlesticksFaker.Generate();
        int IdentityAdded = SUT.AddCandlestick(FakeCandlestick);

        // Act
        int IdentityDeleted = SUT.DeleteCandlestick(FakeCandlestick);

        // Assert
        IdentityDeleted.Should().NotBe(null).And.NotBe(0).And.Be(IdentityAdded);
    }

    [Test, Order(5)]
    public void DeletingFuturesOrder_Works()
    {
        // Arrange
        TVCandlestick FakeCandlestick = CandlesticksFaker.Generate();
        BinanceFuturesOrder FakeFuturesOrder = FuturesOrdersFaker.Generate();
        FakeFuturesOrder.Symbol = FakeCandlestick.CurrencyPair.Name;
        SUT.AddFuturesOrder(FakeFuturesOrder, FakeCandlestick, out int FuturesOrder_Id, out int Candlestick_Id);

        // Act
        new Action(() => SUT.DeleteCandlestick(FakeCandlestick)).Should().Throw<SqlException>($"because the {nameof(FakeFuturesOrder)} depends on the {nameof(FakeCandlestick)}, thus the {nameof(FakeCandlestick)} can't be deleted");
        int DeletedFuturesOrder_Id = SUT.DeleteFuturesOrder(FakeFuturesOrder);

        // Assert
        DeletedFuturesOrder_Id.Should().NotBe(null).And.NotBe(0).And.Be(FuturesOrder_Id);
    }


    [Test, Order(6)]
    public void Database_Creates_Consecutive_IDs()
    {
        // Arrange
        List<int> Candlestick_IDs = new List<int>();
        List<int> FuturesOrders_IDs = new List<int>();


        // Act
        CandlesticksFaker.GenerateBetween(100, 150)
            .DistinctBy(c => (c.CurrencyPair, c.Date)).ToList()
            .ForEach(c => Candlestick_IDs.Add(SUT.AddCandlestick(c)));

        TVCandlestick FakeCandlestick = CandlesticksFaker.Generate();
        FuturesOrdersFaker.GenerateBetween(100, 150)
            .DistinctBy(order => (order.Id, order.Symbol, order.CreateTime)).ToList()
            .ForEach(order =>
            {
                order.Symbol = FakeCandlestick.CurrencyPair.Name;
                SUT.AddFuturesOrder(order, FakeCandlestick, out int id, out int _);
                FuturesOrders_IDs.Add(id);
            });


        // Assert
        Candlestick_IDs.Should().ContainInConsecutiveOrder(Enumerable.Range(Candlestick_IDs.Min(), Candlestick_IDs.Count));
        FuturesOrders_IDs.Should().ContainInConsecutiveOrder(Enumerable.Range(FuturesOrders_IDs.Min(), FuturesOrders_IDs.Count));
    }
    #endregion

    [TearDown]
    public async Task Cleanup()
    {
        await DbRespawner.ResetAsync(ConnectionString);
    }
}
