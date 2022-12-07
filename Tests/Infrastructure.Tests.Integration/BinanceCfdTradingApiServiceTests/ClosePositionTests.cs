using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Binance.Net.Enums;

using CryptoExchange.Net.Objects;

namespace Infrastructure.Tests.Integration.BinanceCfdTradingApiServiceTests;

public class ClosePositionTests : BinanceTradingServiceTestsBase
{
    private bool StopTests = false; // the test execution stops if this field becomes true

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
    public async Task ClosePosition_ClosesLongPosition_WhenLongPositionExists()
    {
        // Arrange
        decimal current_price = await this.SUT.GetCurrentPriceAsync();
        await this.SUT.OpenPositionAtMarketPriceAsync(OrderSide.Buy, this.testMargin, 0.99m * current_price, 1.01m * current_price);

        // Act
        var CallResult = await this.SUT.ClosePositionAsync();

        // Assert
        CallResult.Success.Should().BeTrue();
        CallResult.Data.AvgPrice.Should().BeGreaterThan(0);
        this.SUT.IsInPosition().Should().BeFalse();
    }
    
    [Test, Order(2)]
    public async Task ClosePosition_ClosesShortPosition_WhenShortPositionExists()
    {
        // Arrange
        decimal current_price = await this.SUT.GetCurrentPriceAsync();
        await this.SUT.OpenPositionAtMarketPriceAsync(OrderSide.Sell, this.testMargin, 1.01m * current_price, 0.99m * current_price);
        
        // Act
        var CallResult = await this.SUT.ClosePositionAsync();
        
        // Assert
        CallResult.Success.Should().BeTrue();
        CallResult.Data.AvgPrice.Should().BeGreaterThan(0);
        this.SUT.IsInPosition().Should().BeFalse();
    }

    [Test, Order(3)]
    public async Task ClosePosition_ReturnsNull_WhenPositionDoesNotExist()
    {
        // Act
        var CallResult = await this.SUT.ClosePositionAsync();
        
        // Assert
        CallResult.Success.Should().BeFalse();
        CallResult.Error!.GetType().Should().Be(typeof(ArgumentError));
        this.SUT.IsInPosition().Should().BeFalse();
    }
}
