using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Binance.Net.Enums;

using CryptoExchange.Net.Objects;

namespace Infrastructure.Tests.Integration.BinanceCfdTradingApiServiceTests;

public class OpenPositionTests : BinanceTradingServiceTestsBase
{
    private bool StopTests = false; // the test execution stops if this field becomes true
    private const decimal precision = 1;

    [SetUp]
    public void SetUp() => Assume.That(this.StopTests, Is.False);
    
    [TearDown]
    public async Task TearDown()
    {
        this.StopTests = TestContext.CurrentContext.Result.Outcome.Status != TestStatus.Passed;
        
        if (this.SUT.IsInPosition())
            await this.SUT.ClosePositionAsync();
    }


    
    [Test, Order(1)]
    public async Task OpenPosition_OpensLongPosition_WhenDataIsCorrect()
    {
        // Arrange
        decimal current_price = await this.SUT.GetCurrentPriceAsync();

        // Act
        var CallResult = await this.SUT.OpenPositionAtMarketPriceAsync(OrderSide.Buy, this.testMargin, 0.99m * current_price, 1.01m * current_price);
        
        // Assert
        CallResult.Success.Should().BeTrue();

        this.SUT.IsInPosition().Should().BeTrue();
        this.SUT.Position!.Margin.Should().Be(this.testMargin);
        this.SUT.Position!.StopLossOrder.Should().NotBeNull();
        this.SUT.Position!.TakeProfitOrder.Should().NotBeNull();
        
        this.SUT.Position.StopLossPrice.Should().BeApproximately(0.99m * this.SUT.Position.EntryPrice, precision);
        this.SUT.Position.TakeProfitPrice.Should().BeApproximately(1.01m * this.SUT.Position.EntryPrice, precision);
    }

    [Test, Order(2)]
    public async Task OpenPosition_DoesntOpenLongPosition_WhenDataIsIncorrect()
    {
        // Arrange
        decimal current_price = await this.SUT.GetCurrentPriceAsync();

        // Act
        var CallResult = await this.SUT.OpenPositionAtMarketPriceAsync(OrderSide.Buy, this.testMargin, 1.01m * current_price, 0.99m * current_price);

        // Assert
        CallResult.Success.Should().BeFalse();
        CallResult.Error!.GetType().Should().Be(typeof(ArgumentError));
        this.SUT.IsInPosition().Should().BeFalse();
    }


    [Test, Order(3)]
    public async Task OpenPosition_OpensShortPosition_WhenDataIsCorrect()
    {
        // Arrange
        decimal current_price = await this.SUT.GetCurrentPriceAsync();

        // Act
        var CallResult = await this.SUT.OpenPositionAtMarketPriceAsync(OrderSide.Sell, this.testMargin, 1.01m * current_price, 0.99m * current_price);

        // Assert
        CallResult.Success.Should().BeTrue();
        
        this.SUT.IsInPosition().Should().BeTrue();
        this.SUT.Position!.Margin.Should().Be(this.testMargin);
        this.SUT.Position!.StopLossOrder.Should().NotBeNull();
        this.SUT.Position!.TakeProfitOrder.Should().NotBeNull();
        
        this.SUT.Position.StopLossPrice.Should().BeApproximately(1.01m * this.SUT.Position.EntryPrice, precision);
        this.SUT.Position.TakeProfitPrice.Should().BeApproximately(0.99m * this.SUT.Position.EntryPrice, precision);
    }

    [Test, Order(4)]
    public async Task OpenPosition_DoesntOpenShortPosition_WhenDataIsIncorrect()
    {
        // Arrange
        decimal current_price = await this.SUT.GetCurrentPriceAsync();

        // Act
        var CallResult = await this.SUT.OpenPositionAtMarketPriceAsync(OrderSide.Sell, this.testMargin, 0.99m * current_price, 1.01m * current_price);

        // Assert
        CallResult.Success.Should().BeFalse();
        CallResult.Error!.GetType().Should().Be(typeof(ArgumentError));
        this.SUT.IsInPosition().Should().BeFalse();
    }
}
