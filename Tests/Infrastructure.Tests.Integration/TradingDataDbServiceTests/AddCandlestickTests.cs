using AutomaticDotNETtrading.Infrastructure.TradingStrategies.LuxAlgoAndPSAR.Models;
using System.Data.SqlClient;

namespace Infrastructure.Tests.Integration.TradingDataDbServiceTests;

public class AddCandlestickTests : TradingDataDbServiceTestsBase
{
    [Test, Order(1)]
    public async Task AddCandlestick_AddsCandlestick_WhenCandlestickDoesNotExist()
    {
        // Arrange
        LuxAlgoCandlestick candlestick = this.CandlesticksFaker.Generate();

        // Act
        int id = await this.SUT.AddCandlestickAsync(candlestick);
        
        // Assert
        id.Should().BeGreaterThan(0);
    }
    
    [Test, Order(2)]
    public async Task AddCandlestick_DoesNotAddCandlestick_WhenCandlestickAlreadyExists()
    {
        // Arrange
        LuxAlgoCandlestick candlestick = this.CandlesticksFaker.Generate();
        await this.SUT.AddCandlestickAsync(candlestick);
        
        // Act & Assert
        Func<Task<int>> action = () => this.SUT.AddCandlestickAsync(candlestick);
        await action.Should().ThrowAsync<SqlException>();
    }
}
