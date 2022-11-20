using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomaticDotNETtrading.Infrastructure.TradingStrategies.LuxAlgoAndPSAR.Models;

namespace Infrastructure.Tests.Integration.TradingDataDbServiceTests;

public class DeleteCandlestickTests : TradingDataDbServiceTestsFixture
{
    [Test, Order(1)]
    public async Task DeleteCandlestick_DeletesCandlestick_WhenCandlestickExists()
    {
        // Arrange
        LuxAlgoCandlestick canlestick = this.CandlesticksFaker.Generate();
        int idAdded = await this.SUT.AddCandlestickAsync(canlestick);
        
        // Act
        int idDeleted = await this.SUT.DeleteCandlestickAsync(canlestick);

        // Assert
        idDeleted.Should().Be(idAdded);
    }

    [Test, Order(2)]
    public async Task DeleteCandlestick_ThrowsArgumentException_WhenCandlestickDoesNotExist()
    {
        // Arrange
        LuxAlgoCandlestick canlestick = this.CandlesticksFaker.Generate();
        
        // Assert
        Func<Task<int>> action = async () => await this.SUT.DeleteCandlestickAsync(canlestick);
        await action.Should().ThrowAsync<ArgumentException>();
    }
}
