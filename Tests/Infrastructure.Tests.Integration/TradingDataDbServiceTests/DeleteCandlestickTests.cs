using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AutomaticDotNETtrading.Infrastructure.Models;

namespace Infrastructure.Tests.Integration.TradingDataDbServiceTests;

public class DeleteCandlestickTests : TradingDataDbServiceTestsFixture
{
    [Test, Order(1)]
    public void DeleteCandlestick_DeletesCandlestick_WhenCandlestickExists()
    {
        // Arrange
        TVCandlestick canlestick = this.CandlesticksFaker.Generate();
        int idAdded = this.SUT.AddCandlestick(canlestick);
        
        // Act
        int idDeleted = this.SUT.DeleteCandlestick(canlestick);

        // Assert
        idDeleted.Should().Be(idAdded);
    }

    [Test, Order(2)]
    public void DeleteCandlestick_ThrowsArgumentException_WhenCandlestickDoesNotExist()
    {
        // Arrange
        TVCandlestick canlestick = this.CandlesticksFaker.Generate();

        // Assert
        Action action = () => this.SUT.DeleteCandlestick(canlestick);
        action.Should().Throw<ArgumentException>();
    }
}
