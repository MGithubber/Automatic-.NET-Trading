using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomaticDotNETtrading.Infrastructure.TradingStrategies.LuxAlgoAndPSAR.Models;
using Binance.Net.Objects.Models.Futures;

namespace Infrastructure.Tests.Integration.TradingDataDbServiceTests;

public class DeleteFuturesOrderTests : TradingDataDbServiceTestsFixture
{
    [Test, Order(1)]
    public void DeleteFuturesOrder_DeletesFuturesOrder_IfFuturesOrderExists()
    {
        // Arrange
        TVCandlestick candlestick = this.CandlesticksFaker.Generate();
        BinanceFuturesOrder order = this.FuturesOrdersFaker.Generate();
        order.CreateTime = candlestick.Date;
        order.Symbol = candlestick.CurrencyPair.Name;
        this.SUT.AddFuturesOrder(order, candlestick, out int order_id, out _);

        // Act
        int deleted_order_id = this.SUT.DeleteFuturesOrder(order);

        // Assert
        order_id.Should().Be(deleted_order_id);
    }
    
    [Test, Order(2)]
    public void DeleteFuturesOrder_ThrowsArgumentException_IfFuturesOrderDoesNotExist()
    {
        // Arrange
        BinanceFuturesOrder order = this.FuturesOrdersFaker.Generate();
        
        // Act & Assert
        Action action = () => this.SUT.DeleteFuturesOrder(order);
        action.Should().Throw<ArgumentException>();
    }
}
