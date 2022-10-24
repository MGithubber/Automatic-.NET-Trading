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

    
    // Objects to delete from the database
    private readonly List<object> CleanupList = new List<object>();
    
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
        // Arrange
        TVCandlestick FakeCandlestick = this.CandlesticksFaker.Generate();

        // Act
        int DbScopeIdentity = this.SUT.AddCandlestick(FakeCandlestick);
        this.CleanupList.Add(FakeCandlestick);

        // Assert
        DbScopeIdentity.Should().BeGreaterThan(0);
        new Action(() => this.SUT.AddCandlestick(FakeCandlestick)).Should().ThrowExactly<ArgumentException>("because the candlestick is already in the database");
    }
    
    [Test, Order(4)]
    public void AddingFuturesOrders_Works()
    {
        // Arrange
        TVCandlestick Candlestick1 = this.CandlesticksFaker.Generate();
        BinanceFuturesOrder Order1 = this.BinanceFuturesOrdersFaker.Generate();
        TVCandlestick Candlestick2 = this.CandlesticksFaker.Generate();
        BinanceFuturesOrder Order2 = this.BinanceFuturesOrdersFaker.Generate();
        Order1.CreateTime = Candlestick1.Date;
        Order2.CreateTime = Candlestick2.Date;
        
        if (Candlestick1.Date > Candlestick2.Date)
        {
            (Candlestick1, Candlestick2) = (Candlestick2, Candlestick1);
            (Order1, Order2) = (Order2, Order1);
        }
        

        // Act
        this.SUT.AddFuturesOrder(Order1, Candlestick1, out int Order1_Id, out int Candlestick1_Id);
        this.SUT.AddFuturesOrder(Order2, Candlestick2, out int Order2_Id, out int Candlestick2_Id);
        this.CleanupList.Add(Order1); this.CleanupList.Add(Candlestick1);
        this.CleanupList.Add(Order2); this.CleanupList.Add(Candlestick2);


        // Assert
        Order1_Id.Should().BeGreaterThan(0);
        Candlestick1_Id.Should().BeGreaterThan(0);
        Order2_Id.Should().BeGreaterThan(0);
        Candlestick2_Id.Should().BeGreaterThan(0);
    }
    #endregion
}
