﻿using AutomaticDotNETtrading.Infrastructure.TradingStrategies.LuxAlgoAndPSAR.Models;
using System.Data.SqlClient;

namespace Infrastructure.Tests.Integration.TradingDataDbServiceTests;

public class AddCandlestickTests : TradingDataDbServiceTestsFixture
{
    [Test, Order(1)]
    public void AddCandlestick_AddsCandlestick_WhenCandlestickDoesNotExist()
    {
        // Arrange
        LuxAlgoCandlestick candlestick = this.CandlesticksFaker.Generate();

        // Act
        int id = this.SUT.AddCandlestick(candlestick);
        
        // Assert
        id.Should().BeGreaterThan(0);
    }
    
    [Test, Order(2)]
    public void AddCandlestick_DoesNotAddCandlestick_WhenCandlestickAlreadyExists()
    {
        // Arrange
        LuxAlgoCandlestick candlestick = this.CandlesticksFaker.Generate();
        this.SUT.AddCandlestick(candlestick);
        
        // Act & Assert
        Action action = () => this.SUT.AddCandlestick(candlestick);
        action.Should().Throw<SqlException>();
    }
}
