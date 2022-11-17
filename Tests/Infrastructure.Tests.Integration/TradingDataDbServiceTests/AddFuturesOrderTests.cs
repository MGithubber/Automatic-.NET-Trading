using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AutomaticDotNETtrading.Infrastructure.Models;
using Binance.Net.Objects.Models.Futures;

using System.Data.SqlClient;

namespace Infrastructure.Tests.Integration.TradingDataDbServiceTests;

public class AddFuturesOrderTests : TradingDataDbServiceTestsFixture
{
    [Test, Order(1)]
    public void AddFuturesOrder_AddsFuturesOrder_IfFuturesOrderDoesNotExistAndIsValid()
    {
        // Arrange
        TVCandlestick candlestick = this.CandlesticksFaker.Generate();
        BinanceFuturesOrder order = this.FuturesOrdersFaker.Generate();
        order.CreateTime = candlestick.Date;
        order.Symbol = candlestick.CurrencyPair.Name;

        // Act
        this.SUT.AddFuturesOrder(order, candlestick, out int order_id, out int canlestick_id);

        // Assert
        order_id.Should().BeGreaterThan(0);
        canlestick_id.Should().BeGreaterThan(0);
    }

    [Test, Order(2)]
    public void AddFuturesOrder_DoesNotAddFuturesOrder_AlreadyExists()
    {
        // Arrange
        TVCandlestick candlestick = this.CandlesticksFaker.Generate();
        BinanceFuturesOrder order = this.FuturesOrdersFaker.Generate();
        order.CreateTime = candlestick.Date;
        order.Symbol = candlestick.CurrencyPair.Name;

        // Act
        this.SUT.AddFuturesOrder(order, candlestick, out int order_id, out int canlestick_id);

        // Assert
        Action action = () => this.SUT.AddFuturesOrder(order, candlestick, out _, out _);
        action.Should().Throw<SqlException>();
    }
}
