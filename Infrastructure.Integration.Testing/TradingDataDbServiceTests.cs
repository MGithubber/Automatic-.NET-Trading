using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlConnection = System.Data.SqlClient.SqlConnection;

using AutomaticDotNETtrading.Application.Interfaces.Data;
using AutomaticDotNETtrading.Infrastructure.Data;
using AutomaticDotNETtrading.Infrastructure.Models;
using System.Data.SqlClient;
using System.Data;
using Binance.Net.Objects.Models.Futures;
using Binance.Net.Enums;
using Bogus;

namespace Infrastructure.Integration.Testing;

[TestFixture]
public class TradingDataDbServiceTests
{
    private const string ConnectionString = @"Data Source=(localdb)\MSSQLLocalDB;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Connect Timeout=60;Encrypt=False;TrustServerCertificate=False";
    private const string DatabaseName = "Binance trading logs";
    private IDatabaseConnectionFactory<SqlConnection> ConnectionFactory = new SqlDatabaseConnectionFactory(ConnectionString, DatabaseName);
    private ITradingDataDbService<TVCandlestick> SUT = new TradingDataDbService(ConnectionString, DatabaseName);
    

    #region Fakers
    private readonly Faker<TVCandlestick> CandlesticksFaker = new Faker<TVCandlestick>()
        .RuleFor(c => c.CurrencyPair, f => new CurrencyPair(f.Finance.Currency().Code, f.Finance.Currency().Code))
        .RuleFor(c => c.Date, f => f.Date.Between(f.Date.Recent(5480), f.Date.Soon(5480)))
        .RuleFor(c => c.Open, f => f.Random.Decimal(1000, 3000))
        .RuleFor(c => c.High, f => f.Random.Decimal(1000, 3000))
        .RuleFor(c => c.Low, f => f.Random.Decimal(1000, 3000))
        .RuleFor(c => c.Close, f => f.Random.Decimal(1000, 3000));
    
    private readonly Faker<BinanceFuturesOrder> BinanceFuturesOrdersFaker = new Faker<BinanceFuturesOrder>()
        .RuleFor(order => order.Symbol, f => f.Finance.Currency().Code)
        .RuleFor(order => order.Id, f => f.Random.Long(0, long.MaxValue))
        .RuleFor(order => order.CreateTime, f => f.Date.Between(f.Date.Recent(5480), f.Date.Soon(5480)))
        .RuleFor(order => order.Side, f => f.PickRandom<OrderSide>())
        .RuleFor(order => order.Type, f => f.PickRandom<FuturesOrderType>())
        .RuleFor(order => order.Price, f => f.Random.Decimal(1000, 3000))
        .RuleFor(order => order.Quantity, f => f.Random.Decimal(0.01m, 10));
    #endregion

    
    /// <summary>
    /// Contains all the IDs that are part of this test and must be deleted after the test is finished
    /// </summary>
    private readonly List<int?> CleanupList = new List<int?>();
    
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
        connection.Database.Should().Be(DatabaseName);
        new Action(() => connection.Close()).Should().NotThrow("the connection is open and it can be closed");
    }

    [Test, Order(2)]
    public void AddingCandlestick_Works()
    {
        int DbScopeIdentity = this.SUT.AddCandlestick(this.CandlesticksFaker.Generate());
        DbScopeIdentity.Should().BeGreaterThan(0);
    }

    [Test, Order(3)]
    public void AddingFuturesOrder_Works()
    {
        int DbScopeIdentity = this.SUT.AddFuturesOrder(this.BinanceFuturesOrdersFaker.Generate());
        DbScopeIdentity.Should().BeGreaterThan(0);
    }

    [Test, Order(3)]
    public void AddingFuturesOrderAndCandlestick_Works()
    {
        // Arrange
        TVCandlestick candlestick = this.CandlesticksFaker.Generate();
        BinanceFuturesOrder order = this.BinanceFuturesOrdersFaker.Generate();
        
        // Act
        this.SUT.AddFuturesOrder(order, candlestick, out int FuturesOrder_Identity, out int Candlestick_Identity);

        // Assert
        FuturesOrder_Identity.Should().BeGreaterThan(0);
        Candlestick_Identity.Should().BeGreaterThan(0);
    }

    [Test, Order(4)]
    public void Add_Multiple_FuturesOrders_for_same_Candlestick()
    {
        // Arrange
        TVCandlestick candlestick = this.CandlesticksFaker.Generate();
        BinanceFuturesOrder order = this.BinanceFuturesOrdersFaker.Generate();
        BinanceFuturesOrder orderLater = this.BinanceFuturesOrdersFaker.Generate();
        orderLater.CreateTime = order.CreateTime.AddMinutes(15);
        
        // Act
        this.SUT.AddFuturesOrder(order, candlestick, out int order_identity, out int candlestick_identity);
        this.SUT.AddFuturesOrder(orderLater, candlestick_identity, out int orderLater_identity);

        // Assert
        candlestick_identity.Should().BeGreaterThan(0);
        order_identity.Should().BeGreaterThan(0);
        orderLater_identity.Should().BeGreaterThan(0);
    }
    #endregion
}
