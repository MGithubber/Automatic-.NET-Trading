using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AutomaticDotNETtrading.Infrastructure.Enums;
using AutomaticDotNETtrading.Infrastructure.TradingStrategies.LuxAlgoAndPSAR.Enums;
using AutomaticDotNETtrading.Infrastructure.TradingStrategies.LuxAlgoAndPSAR.Implementations;
using AutomaticDotNETtrading.Infrastructure.TradingStrategies.LuxAlgoAndPSAR.Mapping;
using AutomaticDotNETtrading.Infrastructure.TradingStrategies.LuxAlgoAndPSAR.Models;
using Binance.Net.Objects.Models.Futures;

using Bogus;

using CryptoExchange.Net.Objects;

using NSubstitute.ReturnsExtensions;

namespace Infrastructure.Tests.Unit.LuxAlgoAndPsarTradingStrategyTests.Long;

public class PositionManagementTests : LuxAlgoAndPsarTradingStrategyTestsBase
{
    [SetUp]
    public void OneTimeSetUp() => this.SUT = new LuxAlgoAndPsarTradingStrategyLong(new TradingParameters(), this.BinaceApi);
    


    [Test]
    public void MakeMove_DoesNothing_WhenLastCandlestickHasNoSignal()
    {
        for (int i = 0; i < 2; i++)
        {
            // Arrange
            var stats = new object[] { this.SUT.StoppedOut, this.SUT.EntryPrice, this.SUT.LastTradedSignal, this.SUT.StopLoss, this.SUT.EntryPrice };

            var candles = this.CandlesticksFaker.GenerateBetween(100, 100).ToArray();
            this.SUT.SendData(candles, candles.Last().Close);
            
            if (i == 0)
            {
                this.BinaceApi.Position.Returns(new FuturesPosition(new CurrencyPair("ETH", "BUSD")));
            }
            else
            {
                this.BinaceApi.Position.ReturnsNull();
            }
            this.BinaceApi.IsInPosition().Returns(false);

            using var monitor = this.SUT.Monitor();
            
            // Act
            this.SUT.MakeMove();
            
            // Assert
            monitor.OccurredEvents.Should().BeEmpty();
            stats.Should().BeEquivalentTo(new object[] { this.SUT.StoppedOut, this.SUT.EntryPrice, this.SUT.LastTradedSignal, this.SUT.StopLoss, this.SUT.EntryPrice });
        }
    }

    
    [Test]
    public void MakeMove_OpensPosition_IfLastCandlestickHasBuySignal()
    {
        // Arrange
        var candles = this.CandlesticksFaker.GenerateBetween(100, 100).ToArray();
        candles[^1].Buy = true;
        this.SUT.SendData(candles, candles.Last().Close);
        
        this.BinaceApi.Position.Returns(new FuturesPosition(new CurrencyPair("ETH", "BUSD")));
        this.BinaceApi.IsInPosition().Returns(false);

        using var monitor = this.SUT.Monitor();
        
        // Act
        this.SUT.MakeMove();
        
        // Assert
        monitor.OccurredEvents.Should().HaveCount(1);
        monitor.Should().Raise(nameof(this.SUT.OnPositionOpened));
        this.SUT.TrendDirection.Should().Be(TrendDirection.Uptrend);
        this.SUT.LastTradedSignal.Should().Be(LuxAlgoSignal.Buy);
        this.SUT.StopLoss.Should().BePositive();
    }

    
    [Test]
    public void MakeMove_ClosesPosition_IfLastCandlestickHasSellSignal()
    {
        // Arrange
        this.SUT_OpenPosition();
        
        // append an identical candle to the last one only with a SELL signal
        var candle = this.SUT.Candlesticks.Last();
        candle.Buy = false;
        candle.Sell = true;
        this.SUT.SendData(this.SUT.Candlesticks.Append(candle).ToArray(), candle.Open);
        
        this.BinaceApi.Position.ReturnsNull();
        this.BinaceApi.IsInPosition().Returns(true);
        this.BinaceApi.ClosePositionAsync().Returns(Task.FromResult(new CallResult<BinanceFuturesOrder>(new BinanceFuturesOrder())));

        using var monitor = this.SUT.Monitor();
        
        // Act
        this.SUT.MakeMove();

        // Assert
        monitor.OccurredEvents.Should().HaveCount(1);
        monitor.Should().Raise(nameof(this.SUT.OnPositionClosed));
        this.SUT.TrendDirection.Should().Be(TrendDirection.Downtrend);
        this.SUT.LastTradedSignal.Should().Be(LuxAlgoSignal.Sell);
        this.SUT.StopLoss.Should().Be(0);
    }


    
    private void SUT_OpenPosition()
    {
        var candles = this.CandlesticksFaker.GenerateBetween(100, 100).ToArray();
        candles[^1].Buy = true;
        this.SUT.SendData(candles, candles.Last().Close);

        this.BinaceApi.Position.Returns(new FuturesPosition(new CurrencyPair("ETH", "BUSD")));

        this.SUT.MakeMove();
    }
}
