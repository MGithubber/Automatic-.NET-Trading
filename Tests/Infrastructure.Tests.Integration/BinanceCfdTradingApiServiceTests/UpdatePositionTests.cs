using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Binance.Net.Enums;

using CryptoExchange.Net.Objects;

namespace Infrastructure.Tests.Integration.BinanceCfdTradingApiServiceTests;

public class UpdatePositionTests : BinanceTradingServiceTestsFixture
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
    public async Task UpdateStopLoss_UpdatesStopLoss_WhenPositionExistsAndDataIsCorrect([Random(0.99, 0.999, 1, Distinct = true)] decimal prc)
    {
        // Arrange
        decimal current_price = await this.SUT.GetCurrentPriceAsync();
        await this.SUT.OpenPositionAtMarketPriceAsync(OrderSide.Buy, this.testMargin, 0.99m * current_price, 1.01m * current_price);
        
        // Act
        var CallResult = await this.SUT.PlaceStopLossAsync(prc * current_price);
        
        // Assert
        CallResult.Success.Should().BeTrue();
        CallResult.Data.StopPrice.Should().BeApproximately(prc * current_price, precision);
        this.SUT.Position!.StopLossPrice.Should().BeApproximately(prc * current_price, precision);
        (await this.SUT.GetOrderAsync(this.SUT.Position.StopLossOrder!.Id)).Success.Should().BeTrue();
    }
    
    [Test, Order(2)]
    public async Task UpdateStopLoss_DoesntUpdatesStopLoss_WhenPositionExistsButDataIsIncorrect([Random(1.001, 1.01, 1, Distinct = true)] decimal prc)
    {
        // Arrange
        decimal current_price = await this.SUT.GetCurrentPriceAsync();
        await this.SUT.OpenPositionAtMarketPriceAsync(OrderSide.Buy, this.testMargin, 0.99m * current_price, 1.01m * current_price);
        decimal initial_stop_loss = this.SUT.Position!.StopLossPrice!.Value;
        
        // Act
        var CallResult = await this.SUT.PlaceStopLossAsync(-1);

        // Assert
        CallResult.Success.Should().BeFalse();
        CallResult.Error!.GetType().Should().Be(typeof(ServerError));
        this.SUT.Position!.StopLossPrice.Should().Be(initial_stop_loss);
        (await this.SUT.GetOrderAsync(this.SUT.Position.StopLossOrder!.Id)).Success.Should().BeTrue();
    }
    
    [Test, Order(3)]
    public async Task UpdateStopLoss_DoesntUpdatesStopLoss_WhenPositionDoesNotExist()
    {
        // Act
        var CallResult = await this.SUT.PlaceStopLossAsync(-1);

        // Assert
        CallResult.Success.Should().BeFalse();
        CallResult.Error!.GetType().Should().Be(typeof(ArgumentError));
        this.SUT.Position!.Should().BeNull();
    }
}
