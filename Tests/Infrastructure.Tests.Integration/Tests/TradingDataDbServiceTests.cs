using System.Data.SqlClient;

using AutomaticDotNETtrading.Application.Interfaces.Data;
using AutomaticDotNETtrading.Infrastructure.Data;
using AutomaticDotNETtrading.Infrastructure.Enums;
using AutomaticDotNETtrading.Infrastructure.Models;

using Binance.Net.Enums;
using Binance.Net.Objects.Models.Futures;

using Bogus;

namespace Infrastructure.Tests.Integration.Tests;

[TestFixture]
public class TradingDataDbServiceTests
{
    private const string ConnectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=""Binance trading logs"";Integrated Security=True";
    private readonly IDatabaseConnectionFactory<SqlConnection> ConnectionFactory = new SqlDatabaseConnectionFactory(ConnectionString);
    private readonly ITradingDataDbService<TVCandlestick> SUT = new TradingDataDbService(ConnectionString);


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

            switch (f.PickRandom<LuxAlgoSignal>())
            {
                case LuxAlgoSignal.Buy: c.Buy = true; break;
                case LuxAlgoSignal.StrongBuy: c.StrongBuy = true; break;
                case LuxAlgoSignal.Sell: c.Sell = true; break;
                case LuxAlgoSignal.StrongSell: c.StrongSell = true; break;
                case LuxAlgoSignal.ExitBuy: c.ExitBuy = f.Random.Double(950, 3050); break;
                case LuxAlgoSignal.ExitSell: c.ExitSell = f.Random.Double(950, 3050); break;
            }
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
    
    #region private methods
    private TVCandlestick GetFakeCandlestick()
    {
        TVCandlestick fake = this.CandlesticksFaker.Generate();
        this.CleanupCandlesticks.Add(fake);
        return fake;
    }
    private List<TVCandlestick> GetFakeCandlesticks(int min, int max)
    {
        List<TVCandlestick> fakes = this.CandlesticksFaker.GenerateBetween(min, max);
        fakes.ForEach(fake => this.CleanupCandlesticks.Add(fake));
        return fakes;
    }

    private BinanceFuturesOrder GetFakeFuturesOrder()
    {
        BinanceFuturesOrder fake = this.FuturesOrdersFaker.Generate();
        this.CleanupFuturesOrders.Add(fake);
        return fake;
    }
    private List<BinanceFuturesOrder> GetFakeFuturesOrders(int min, int max)
    {
        List<BinanceFuturesOrder> fakes = this.FuturesOrdersFaker.GenerateBetween(min, max);
        fakes.ForEach(fake => this.CleanupFuturesOrders.Add(fake));
        return fakes;
    }
    #endregion

    // Objects to delete from the database
    private readonly List<TVCandlestick> CleanupCandlesticks = new();
    private readonly List<BinanceFuturesOrder> CleanupFuturesOrders = new();
    
    
    #region Tests
    [Test, Order(1)]
    public void CreatingAndClosingConnection_Work()
    {
        // Arrange
        SqlConnection connection = default!;
        Action createConnection = new Action(() => connection = this.ConnectionFactory.CreateConnection());


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
        TVCandlestick FakeCandlestick = this.GetFakeCandlestick();
        Action action = new Action(() => FakeCandlestick_db_id = this.SUT.AddCandlestick(FakeCandlestick));

        // Act & Assert
        action.Should().NotThrow("because the candlestick is not in the database yet and can be added");
        FakeCandlestick_db_id.Should().BeGreaterThan(0);
        action.Should().Throw<Exception>("because the candlestick is in the database already and can't be added");
    }

    [Test, Order(3)]
    public void AddingFuturesOrders_Works()
    {
        // Arrange
        TVCandlestick Candlestick1 = this.GetFakeCandlestick();
        BinanceFuturesOrder Order1 = this.GetFakeFuturesOrder();
        Order1.CreateTime = Candlestick1.Date;
        Order1.Symbol = Candlestick1.CurrencyPair.Name;

        TVCandlestick Candlestick2 = this.GetFakeCandlestick();
        BinanceFuturesOrder Order2 = this.GetFakeFuturesOrder();
        Order2.CreateTime = Candlestick2.Date;
        Order2.Symbol = Candlestick2.CurrencyPair.Name;

        if (Candlestick1.Date > Candlestick2.Date)
        {
            (Candlestick1, Candlestick2) = (Candlestick2, Candlestick1);
            (Order1, Order2) = (Order2, Order1);
        }

        BinanceFuturesOrder Order3 = this.GetFakeFuturesOrder();
        Order3.Symbol = Candlestick2.CurrencyPair.Name;


        // Act
        this.SUT.AddFuturesOrder(Order1, Candlestick1, out int Order1_Id, out int Candlestick1_Id);
        this.SUT.AddFuturesOrder(Order2, Candlestick2, out int Order2_Id, out int Candlestick2_Id);
        this.SUT.AddFuturesOrder(Order3, Candlestick2, out int Order3_Id, out int Candlestick3_Id);


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
        TVCandlestick FakeCandlestick = this.GetFakeCandlestick();
        int IdentityAdded = this.SUT.AddCandlestick(FakeCandlestick);

        // Act
        int IdentityDeleted = this.SUT.DeleteCandlestick(FakeCandlestick);

        // Assert
        IdentityDeleted.Should().NotBe(null).And.NotBe(0).And.Be(IdentityAdded);
    }

    [Test, Order(5)]
    public void DeletingFuturesOrder_Works()
    {
        // Arrange
        TVCandlestick FakeCandlestick = this.GetFakeCandlestick();
        BinanceFuturesOrder FakeFuturesOrder = this.GetFakeFuturesOrder();
        FakeFuturesOrder.Symbol = FakeCandlestick.CurrencyPair.Name;
        this.SUT.AddFuturesOrder(FakeFuturesOrder, FakeCandlestick, out int FuturesOrder_Id, out int Candlestick_Id);

        // Act
        new Action(() => this.SUT.DeleteCandlestick(FakeCandlestick)).Should().Throw<SqlException>($"because the {nameof(FakeFuturesOrder)} depends on the {nameof(FakeCandlestick)}, thus the {nameof(FakeCandlestick)} can't be deleted");
        int DeletedFuturesOrder_Id = this.SUT.DeleteFuturesOrder(FakeFuturesOrder);

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
        this.GetFakeCandlesticks(100, 150)
            .DistinctBy(c => (c.CurrencyPair, c.Date)).ToList()
            .ForEach(c => Candlestick_IDs.Add(this.SUT.AddCandlestick(c)));

        TVCandlestick FakeCandlestick = this.GetFakeCandlestick();
        this.GetFakeFuturesOrders(100, 150)
            .DistinctBy(order => (order.Id, order.Symbol, order.CreateTime)).ToList()
            .ForEach(order =>
            {
                order.Symbol = FakeCandlestick.CurrencyPair.Name;
                this.SUT.AddFuturesOrder(order, FakeCandlestick, out int id, out int _);
                FuturesOrders_IDs.Add(id);
            });


        // Assert
        Candlestick_IDs.Should().ContainInConsecutiveOrder(Enumerable.Range(Candlestick_IDs.Min(), Candlestick_IDs.Count));
        FuturesOrders_IDs.Should().ContainInConsecutiveOrder(Enumerable.Range(FuturesOrders_IDs.Min(), FuturesOrders_IDs.Count));
    }
    #endregion
    
    [OneTimeTearDown]
    public void Cleanup()
    {
        static bool TryInvoke(Func<object> func)
        {
            try { func.Invoke(); return true; }
            catch { return false; }
        }
        
        SqlConnection connection = this.ConnectionFactory.CreateConnection();
        try
        {
            // the BinanceFuturesOrders are deleted first because the candlesticks are depending on them
            this.CleanupFuturesOrders.ForEach(order => TryInvoke(() => this.SUT.DeleteFuturesOrder(order)));
            this.CleanupCandlesticks.ForEach(candle => TryInvoke(() => this.SUT.DeleteCandlestick(candle)));
            
            this.CleanupFuturesOrders.Clear();
            this.CleanupCandlesticks.Clear();
        }
        finally { connection?.Close(); }
    }
}
