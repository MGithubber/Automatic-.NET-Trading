using AutomaticDotNETtrading.Infrastructure.Models;

using System.Data.SqlClient;

namespace Infrastructure.Tests.Integration.TradingDataDbServiceTests;

public class AddCandlestickTests : TradingDataDbServiceTestsFixture
{
    [Test, Order(1)]
    public void AddCandlestick_AddsCandlestick_WhenCandlestickDoesNotExist()
    {
        // Arrange
        TVCandlestick candlestick = this.CandlesticksFaker.Generate();

        // Act
        int id = this.SUT.AddCandlestick(candlestick);
        
        // Assert
        id.Should().BeGreaterThan(0);
    }
    
    [Test, Order(2)]
    public void AddCandlestick_DoesNotAddCandlestick_WhenCandlestickAlreadyExists()
    {
        // Arrange
        TVCandlestick candlestick = this.CandlesticksFaker.Generate();
        this.SUT.AddCandlestick(candlestick);
        
        // Act & Assert
        Action action = () => this.SUT.AddCandlestick(candlestick);
        action.Should().Throw<SqlException>();
    }
}
