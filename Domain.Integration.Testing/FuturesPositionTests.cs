using Binance.Net.Enums;
using Binance.Net.Objects.Models.Futures;

namespace Domain.Integration.Testing;

[TestFixture]
public class FuturesPositionTests
{
    private FuturesPosition? FuturesPosition { get; set; }

    [Test, Order(1)]
    public void FuturesPosition_Setters_Throw_At_Wrong_Input()
    {
        // Arrange
        this.FuturesPosition = new FuturesPosition(new CurrencyPair("ETH", "BUSD"))
        {
            EntryOrder = new BinanceFuturesOrder()
            {
                Symbol = "ETHBUSD",
                Type = FuturesOrderType.Market
            },
            StopLossOrder = null,
            TakeProfitOrder = null,
        };
        
        
        // Act
        // Assert
        
        Assert.Throws<ArgumentException>(() => this.FuturesPosition.StopLossOrder = new BinanceFuturesOrder
        {
            Symbol = "ETHBUSD",
            Type = FuturesOrderType.Market,
        });
        Assert.Throws<ArgumentException>(() => this.FuturesPosition.TakeProfitOrder = new BinanceFuturesOrder
        {
            Symbol = "Incorrect Symbol",
            Type = FuturesOrderType.StopMarket,
        });

        Assert.DoesNotThrow(() => this.FuturesPosition.StopLossOrder = new BinanceFuturesOrder
        {
            Symbol = "ETHBUSD",
            Type = FuturesOrderType.Limit,
        });
        Assert.DoesNotThrow(() => this.FuturesPosition.TakeProfitOrder = new BinanceFuturesOrder
        {
            Symbol = "ETHBUSD",
            Type = FuturesOrderType.StopMarket,
        });
    }
}
