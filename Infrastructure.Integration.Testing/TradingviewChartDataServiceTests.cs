using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestPlatform.TestHost;
using OpenQA.Selenium;

namespace Infrastructure.Integration.Testing;

[TestFixture]
public class TradingviewChartDataServiceTests
{
    private TradingviewChartDataService SUT; // = TO DO


    #region Tests
    [Test, Order(1)]
    public void Returns_candlestick_when_DataWindowText_is_correct()
    {
        // Arrange
        // // assign data window correct text // //

        // Act
        // // get the candle // //

        // Assert
        // // candlestick should be ok // //
    }

    [Test, Order(2)]
    public void Properly_waits_until_the_DataWindowText_is_correct()
    {
        // Arrange


        // Act


        // Assert

    }
    #endregion
}
