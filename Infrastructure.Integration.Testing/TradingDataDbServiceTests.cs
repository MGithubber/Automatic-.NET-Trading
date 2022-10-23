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

namespace Infrastructure.Integration.Testing;

[TestFixture]
public class TradingDataDbServiceTests
{
    private const string ConnectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=\"Binance trading logs\";Integrated Security=True;Connect Timeout=60;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
    private const string DatabaseName = "Binance trading logs";
    private IDatabaseConnectionFactory<SqlConnection> smallSUT = new SqlDatabaseConnectionFactory(ConnectionString, DatabaseName);
    private ITradingDataDbService<TVCandlestick> SUT = new TradingDataDbService(ConnectionString, DatabaseName);
    

    #region Tests
    [Test, Order(1)]
    public void CreatingAndClosingConnection_Work()
    {
        // Arrange
        SqlConnection connection = default!;
        Action createConnection = new Action(() => connection = this.smallSUT.CreateConnection());

        
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
        int DbScopeIdentity = this.SUT.AddCandlestick(new TVCandlestick
        {
            CurrencyPair = new CurrencyPair("ETH", "BUSD"),
            Date = DateTime.Now,
            Open = 69420.0m,
            High = 69420.0m,
            Low = 69420.0m,
            Close = 69420.0m,
        });
        
        DbScopeIdentity.Should().BeGreaterThan(0);
    }

    [Test, Order(3)]
    public void AddingFuturesOrder_Works()
    {
        int DbScopeIdentity = this.SUT.AddFuturesOrder(new BinanceFuturesOrder
        {
            Symbol = "ETHBUSD",
            Id = Guid.NewGuid().GetHashCode(),
            CreateTime = DateTime.Now,
            Side = OrderSide.Buy,
            Type = FuturesOrderType.Market,
            Price = 420,
            Quantity = 69
        });
        
        DbScopeIdentity.Should().BeGreaterThan(0);
    }

    [Test, Order(3)]
    public void AddingFuturesOrderAndCandlestick_Works()
    {
        this.SUT.AddFuturesOrder(
        new BinanceFuturesOrder
        {
            Symbol = "ETHBUSD",
            Id = Guid.NewGuid().GetHashCode(),
            CreateTime = DateTime.Now,
            Side = OrderSide.Buy,
            Type = FuturesOrderType.Market,
            Price = 420,
            Quantity = 69
        },
        new TVCandlestick
        {
            CurrencyPair = new CurrencyPair("ETH", "BUSD"),
            Date = DateTime.Now,
            Open = 69420.0m,
            High = 69420.0m,
            Low = 69420.0m,
            Close = 69420.0m,
        }, 
        out int FuturesOrder_Identity, out int Candlestick_Identity);

        FuturesOrder_Identity.Should().BeGreaterThan(0);
        Candlestick_Identity.Should().BeGreaterThan(0);
    }
    #endregion
}
