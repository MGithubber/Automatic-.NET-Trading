using Binance.Net.Enums;
using Binance.Net.Objects.Models.Futures;

namespace Domain.Tests.Tests;

[TestFixture]
public class FuturesPositionTests
{
    private FuturesPosition FuturesPosition = default!;

    
    [Test, Order(1)]
    public void FuturesPositionSetters_ShouldNotThrow_WhenInputIsCorrect()
    {
        // Arrange
        var currencyPair = new CurrencyPair("ETH", "BUSD");
        var entryOrder = new BinanceFuturesOrder()
        {
            Symbol = "ETHBUSD",
            Type = FuturesOrderType.Market
        };
        this.FuturesPosition = new FuturesPosition(currencyPair)
        {
            EntryOrder = entryOrder,
            StopLossOrder = null,
            TakeProfitOrder = null,
        };
        
        Action SetOkStopLoss = new Action(() => this.FuturesPosition.StopLossOrder = new BinanceFuturesOrder
        {
            Symbol = "ETHBUSD",
            Type = FuturesOrderType.Limit,
        });
        Action SetOkTakeProfit = new Action(() => this.FuturesPosition.TakeProfitOrder = new BinanceFuturesOrder
        {
            Symbol = "ETHBUSD",
            Type = FuturesOrderType.StopMarket,
        });

        // Act
        // Assert
        SetOkStopLoss.Should().NotThrow();
        SetOkTakeProfit.Should().NotThrow();
    }

    [Test, Order(2)]
    public void FuturesPositionSetters_ShouldThrow_WhenInputIsWrong()
    {
        // Arrange
        var currencyPair = new CurrencyPair("ETH", "BUSD");
        var entryOrder = new BinanceFuturesOrder()
        {
            Symbol = "ETHBUSD",
            Type = FuturesOrderType.Market
        };
        this.FuturesPosition = new FuturesPosition(currencyPair)
        {
            EntryOrder = entryOrder,
            StopLossOrder = null,
            TakeProfitOrder = null,
        };

        Action SetWrongStopLoss = new Action(() => this.FuturesPosition.StopLossOrder = new BinanceFuturesOrder
        {
            Symbol = "ETHBUSD",
            Type = FuturesOrderType.Market,
        });
        Action SetTakeProfit = new Action(() => this.FuturesPosition.TakeProfitOrder = new BinanceFuturesOrder
        {
            Symbol = "Incorrect Symbol",
            Type = FuturesOrderType.StopMarket,
        });

        // Act
        // Assert
        SetWrongStopLoss.Should().Throw<ArgumentException>();
        SetTakeProfit.Should().Throw<ArgumentException>();
    }
}
