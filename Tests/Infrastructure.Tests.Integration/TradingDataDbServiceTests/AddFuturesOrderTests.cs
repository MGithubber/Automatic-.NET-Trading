using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Binance.Net.Objects.Models.Futures;

using System.Data.SqlClient;
using AutomaticDotNETtrading.Infrastructure.TradingStrategies.LuxAlgoAndPSAR.Models;

namespace Infrastructure.Tests.Integration.TradingDataDbServiceTests;

public class AddFuturesOrderTests : TradingDataDbServiceTestsFixture
{
    [Test, Order(1)]
    public async Task AddFuturesOrder_AddsFuturesOrder_IfFuturesOrderDoesNotExistAndIsValid()
    {
        // Arrange
        LuxAlgoCandlestick candlestick = this.CandlesticksFaker.Generate();
        BinanceFuturesOrder order = this.FuturesOrdersFaker.Generate();
        order.CreateTime = candlestick.Date;
        order.Symbol = candlestick.CurrencyPair.Name;

        // Act
        (int order_id, int canlestick_id) = await this.SUT.AddFuturesOrderAsync(order, candlestick);
        
        // Assert
        order_id.Should().BeGreaterThan(0);
        canlestick_id.Should().BeGreaterThan(0);
    }

    [Test, Order(2)]
    public async Task AddFuturesOrder_DoesNotAddFuturesOrder_AlreadyExists()
    {
        // Arrange
        LuxAlgoCandlestick candlestick = this.CandlesticksFaker.Generate();
        BinanceFuturesOrder order = this.FuturesOrdersFaker.Generate();
        order.CreateTime = candlestick.Date;
        order.Symbol = candlestick.CurrencyPair.Name;

        // Act
        var (FuturesOrder_Id, Candlestick_Id) = await this.SUT.AddFuturesOrderAsync(order, candlestick);
        
        // Assert
        Func<Task> action = async () => await this.SUT.AddFuturesOrderAsync(order, candlestick);
        await action.Should().ThrowAsync<SqlException>();
    }
}
