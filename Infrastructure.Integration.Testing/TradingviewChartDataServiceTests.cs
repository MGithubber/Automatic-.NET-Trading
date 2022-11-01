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

    
    [OneTimeSetUp]
    public void Setup()
    {
        this.SUT = new TradingviewChartDataService(
                chromeDriverDirectory: @"..\Process files\Chrome driver",
                userDataDirectory: $@"C:\Users\{Environment.UserName}\AppData\Local\Google\Chrome\User Data",
                downloadsDirectory: @"..\Process files\Chrome driver\Downloads",
                Chart_Locator: By.ClassName("chart-gui-wrapper"),
                DataWindow_Locator: By.ClassName("chart-data-window"),
                ZoomInButton_Locator: By.ClassName("control-bar__btn--zoom-in"),
                ZoomOutButton_Locator: By.ClassName("control-bar__btn--zoom-out"),
                ScrollLeftButton_Locator: By.ClassName("control-bar__btn--move-left"),
                ScrollRightButton_Locator: By.ClassName("control-bar__btn--move-right"),
                ResetChartButton_Locator: By.ClassName("control-bar__btn--turn-button"),
                ManageLayoutsButton_Locator: By.ClassName("js-save-load-menu-open-button"),
                ExportChartDataButton_Locator: By.XPath(@"/html[@class='is-authenticated is-pro is-not-trial theme-dark is-upgrade-available feature-no-touch feature-no-mobiletouch is-not-trial-available']/body[@class='chart-page unselectable i-no-scroll']/div[@id='overlap-manager-root']/div/span/div[@class='menuWrap-biWYdsXC']/div[@class='scrollWrap-biWYdsXC momentumBased-biWYdsXC']/div[@class='menuBox-biWYdsXC']/div[@class='item-RhC5uhZw withIcon-RhC5uhZw withIcon-PSZfKmCG'][3]"),
                ExportChartDataConfirmButton_Locator: By.XPath(@"/html[@class='is-authenticated is-pro is-not-trial theme-dark is-upgrade-available feature-no-touch feature-no-mobiletouch is-not-trial-available']/body[@class='chart-page unselectable i-no-scroll']/div[@id='overlap-manager-root']/div/div/div[@class='dialog-UExGRfA_ dialog-o2xKpnz8 dialog-nnDbXk_L rounded-nnDbXk_L shadowed-nnDbXk_L']/div[@class='wrapper-o2xKpnz8']/div[@class='footer-PQhX1JKt']/div[@class='buttons-PQhX1JKt']/span[@class='submitButton-PQhX1JKt']/button[@class='button-OvB35Th_ size-small-OvB35Th_ color-brand-OvB35Th_ variant-primary-OvB35Th_']"));
    }


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
