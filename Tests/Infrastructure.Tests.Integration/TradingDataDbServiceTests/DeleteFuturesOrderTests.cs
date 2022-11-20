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
    public async Task DeleteFuturesOrder_DeletesFuturesOrder_IfFuturesOrderExists()
    {
        // Arrange
        LuxAlgoCandlestick candlestick = this.CandlesticksFaker.Generate();
        BinanceFuturesOrder order = this.FuturesOrdersFaker.Generate();
        order.CreateTime = candlestick.Date;
        order.Symbol = candlestick.CurrencyPair.Name;
        (int order_id, _) = await this.SUT.AddFuturesOrderAsync(order, candlestick);
        
        // Act
        int deleted_order_id = await this.SUT.DeleteFuturesOrderAsync(order);

        // Assert
        order_id.Should().Be(deleted_order_id);
    }
    
    [Test, Order(2)]
    public async Task DeleteFuturesOrder_ThrowsArgumentException_IfFuturesOrderDoesNotExist()
    {
        // Arrange
        BinanceFuturesOrder order = this.FuturesOrdersFaker.Generate();
         
        // Act & Assert
        Func<Task<int>> action = async () => await this.SUT.DeleteFuturesOrderAsync(order);
        await action.Should().ThrowAsync<ArgumentException>();
    }
}
