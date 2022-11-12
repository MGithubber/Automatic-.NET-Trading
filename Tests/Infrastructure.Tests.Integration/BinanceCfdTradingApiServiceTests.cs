using System.Reflection;

using Binance.Net.Enums;

namespace Infrastructure.Tests.Integration;

[TestFixture]
public class BinanceCfdTradingApiServiceTests
{
    private bool StopTests; // the test execution stops if this field becomes true

    private readonly ApiCredentials BinanceApiCredentials = new ApiCredentials("IctW96tLRARy1J7EeW5BDvlxhihbF30uquSNEAoBm0otz1WsWZW8WgZ9wE8n9fsQ", "5bXCRQgUrqhPFEU4nAYaEMGN3EhrkaTvuKLy7diK22LRs8jLFdYdXL57eshUUyre");
    private readonly CurrencyPair CurrencyPair = new CurrencyPair("ETH", "BUSD");
    private ICfdTradingApiService SUT { get; set; } = default!;
    private decimal testMargin = 3;

    [OneTimeSetUp]
    public void OneTimeSetUp() => SUT = new BinanceCfdTradingApiService(CurrencyPair, BinanceApiCredentials);

    [SetUp]
    public void SetUp() => Assume.That(StopTests, Is.False);

    //// //// ////

    #region Tests
    [Test, Order(1)]
    public async Task OpenLongPosition_Works()
    {
        // Act
        decimal current_price = await SUT.GetCurrentPriceAsync();
        bool success = (await SUT.OpenPositionAtMarketPriceAsync(OrderSide.Buy, testMargin, 0.99m * current_price, 1.01m * current_price)).Success;

        // Assert
        success.Should().BeTrue();
        SUT.IsInPosition().Should().BeTrue("this.SUT.OpenPositionAtMarketPrice method has just been called");
        SUT.Position!.StopLossOrder.Should().NotBeNull("a stop loss price has been specified");
        SUT.Position!.TakeProfitOrder.Should().NotBeNull("a stop loss price has been specified");
    }

    [Test, Order(2)]
    public async Task Update_LongPosition_StopLoss_Works([Random(0.99, 0.999, 5)] decimal procent_times_current_price)
    {
        // Act
        decimal current_price = await SUT.GetCurrentPriceAsync();
        Assert.True((await SUT.PlaceStopLossAsync(procent_times_current_price * current_price)).Success, "Stop loss placing failed");

        // Assert
        Assert.True(SUT.IsInPosition(), "Failed to determine if ICfdTradingApiService is in position or not");
        Assert.IsNotNull(SUT.Position!.StopLossOrder, $"{nameof(SUT.Position.StopLossOrder)} is NULL after stop loss placing appears as beeing succesful");
        Assert.True(SUT.Position!.StopLossOrder.StopPrice == Math.Round(procent_times_current_price * current_price, 2));
        Assert.True((await SUT.GetOrderAsync(SUT.Position!.StopLossOrder.Id)).Success, "Unable to get back the stop loss information");
        Assert.True((await SUT.GetOrderAsync(SUT.Position!.StopLossOrder)).Data.StopPrice == Math.Round(procent_times_current_price * current_price, 2));
    }

    [Test, Order(3)]
    public async Task CloseLongPosition_Works()
    {
        // Act
        Assert.True((await SUT.ClosePositionAsync()).Success, "Position closing failed");

        // Assert
        Assert.False(SUT.IsInPosition(), $"Failed to determine if {typeof(ICfdTradingApiService).Name} is in position or not");
    }


    [Test, Order(4)]
    public async Task OpenShortPosition_Works()
    {
        // Act
        decimal current_price = await SUT.GetCurrentPriceAsync();
        bool success = (await SUT.OpenPositionAtMarketPriceAsync(OrderSide.Sell, testMargin, 1.01m * current_price, 0.99m * current_price)).Success;

        // Assert
        success.Should().BeTrue();
        SUT.IsInPosition().Should().BeTrue("this.SUT.OpenPositionAtMarketPrice method has just been called");
        SUT.Position!.StopLossOrder.Should().NotBeNull("a stop loss price has been specified");
        SUT.Position!.TakeProfitOrder.Should().NotBeNull("a stop loss price has been specified");
    }

    [Test, Order(5)]
    public async Task Update_ShortPosition_StopLoss_Works([Random(1.001, 1.01, 5)] decimal procent_times_current_price)
    {
        // Act
        decimal current_price = await SUT.GetCurrentPriceAsync();
        Assert.True((await SUT.PlaceStopLossAsync(procent_times_current_price * current_price)).Success, "Stop loss placing failed");

        // Assert
        Assert.True(SUT.IsInPosition(), "Failed to determine if ICfdTradingApiService is in position or not");
        Assert.IsNotNull(SUT.Position!.StopLossOrder, $"{nameof(SUT.Position.StopLossOrder)} is NULL after stop loss placing appears as beeing succesful");
        Assert.True(SUT.Position!.StopLossOrder.StopPrice == Math.Round(procent_times_current_price * current_price, 2));
        Assert.True((await SUT.GetOrderAsync(SUT.Position!.StopLossOrder.Id)).Success, "Unable to get back the stop loss information");
        Assert.True((await SUT.GetOrderAsync(SUT.Position!.StopLossOrder)).Data.StopPrice == Math.Round(procent_times_current_price * current_price, 2));
    }

    [Test, Order(6)]
    public async Task CloseShortPosition_Works()
    {
        // Act
        Assert.True((await SUT.ClosePositionAsync()).Success, "Position closing failed");

        // Assert
        Assert.False(SUT.IsInPosition(), $"Failed to determine if {typeof(ICfdTradingApiService).Name} is in position or not");
    }
    #endregion

    //// //// ////

    [OneTimeTearDown]
    public void OneTimeTearDown() { }

    [TearDown]
    public void TearDown() => StopTests = TestContext.CurrentContext.Result.Outcome.Status != TestStatus.Passed;
}
