using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

// // // SELENIUM .NET // // //
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

// // // CSV HELPER .NET // // //
using CsvHelper;
using CsvHelper.Configuration;
using OpenQA.Selenium.Interactions;
using AutomaticDotNETtrading.Application.Interfaces.Services;
using AutomaticDotNETtrading.Domain.Models;
using System.Reflection;
using System.Text;
using AutomaticDotNETtrading.Infrastructure.TradingStrategies.LuxAlgoAndPSAR.Models;
using AutomaticDotNETtrading.Infrastructure.TradingStrategies.LuxAlgoAndPSAR.Mapping;

namespace AutomaticDotNETtrading.Infrastructure.Services;


internal static class IWebDriverExtensions
{
    /// <summary>
    /// Sets the cursor to the specified location
    /// </summary>
    public static void MoveCursorToLocation(this IWebDriver webDriver, int offsetX, int offsetY) => new Actions(webDriver).MoveByOffset(offsetX, offsetY).Perform();

    /// <summary>
    /// Sets the cursor to the specified location and performs a click
    /// </summary>
    public static void MoveCursorToLocationAndClick(this IWebDriver webDriver, int offsetX, int offsetY) => new Actions(webDriver).MoveByOffset(offsetX, offsetY).Click().Perform();

    /// <summary>
    /// Sets the cursor to the specified location with respect to the specified web element's origin
    /// </summary>
    public static void MoveCursorToLocationOnElement(this IWebDriver webDriver, IWebElement element, int offsetX, int offsetY) => new Actions(webDriver).MoveToElement(element, offsetX, offsetY).Perform();

    /// <summary>
    /// Sets the cursor to the specified location with respect to the specified web element's origin and performs a click
    /// </summary>
    public static void MoveCursorToLocationOnElementAndClick(this IWebDriver webDriver, IWebElement element, int offsetX, int offsetY) => new Actions(webDriver).MoveToElement(element, offsetX, offsetY).Perform();
}

//// //// //// //// //// ////

/// <summary>
/// Represents an immutable object providing methods for extracting chart data from https://www.tradingview.com using google chrome
/// </summary>
public class TradingviewChartDataService : IChartDataService<LuxAlgoCandlestick>
{
    private readonly ChromeOptions ChromeOptions;
    private readonly ChromeDriver ChromeDriver;
    private readonly string downloadsDirectory;

    #region Locators
    private readonly By DataWindow_Locator;
    private readonly By Chart_Locator;

    private readonly By ZoomInButton_Locator;
    private readonly By ZoomOutButton_Locator;
    private readonly By ScrollLeftButton_Locator;
    private readonly By ScrollRightButton_Locator;
    private readonly By ResetChartButton_Locator;

    private readonly By ManageLayoutsButton_Locator;
    private readonly By ExportChartDataButton_Locator;
    private readonly By ExportChartDataConfirmButton_Locator;
    #endregion
    
    internal TradingviewChartDataService() { }
    public TradingviewChartDataService(string chromeDriverDirectory, string userDataDirectory, string downloadsDirectory, By Chart_Locator, By DataWindow_Locator, By ZoomInButton_Locator, By ZoomOutButton_Locator, By ScrollLeftButton_Locator, By ScrollRightButton_Locator, By ResetChartButton_Locator, By ManageLayoutsButton_Locator, By ExportChartDataButton_Locator, By ExportChartDataConfirmButton_Locator)
    {
        this.downloadsDirectory = downloadsDirectory;

        this.ChromeOptions = new ChromeOptions();
        this.ChromeOptions.AddArgument($@"user-data-dir={userDataDirectory}");
        this.ChromeOptions.AddArgument("--log-level=3");
        this.ChromeOptions.AddUserProfilePreference("download.default_directory", downloadsDirectory);
        this.ChromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
        this.ChromeOptions.AddUserProfilePreference("disable-popup-blocking", true);
        this.ChromeOptions.AddUserProfilePreference("block_third_party_cookies", true);
        
        this.ChromeDriver = new ChromeDriver(chromeDriverDirectory, this.ChromeOptions);
        this.ChromeDriver.Url = "https://www.tradingview.com/chart/oxqzhJn4/?symbol=BINANCE%3AETHBUSD";

        #region Locators
        this.Chart_Locator = Chart_Locator;
        this.DataWindow_Locator = DataWindow_Locator;

        this.ZoomInButton_Locator = ZoomInButton_Locator;
        this.ZoomOutButton_Locator = ZoomOutButton_Locator;
        this.ScrollLeftButton_Locator = ScrollLeftButton_Locator;
        this.ScrollRightButton_Locator = ScrollRightButton_Locator;
        this.ResetChartButton_Locator = ResetChartButton_Locator;

        this.ManageLayoutsButton_Locator = ManageLayoutsButton_Locator;
        this.ExportChartDataButton_Locator = ExportChartDataButton_Locator;
        this.ExportChartDataConfirmButton_Locator = ExportChartDataConfirmButton_Locator;
        #endregion

        this.WebWait = new WebDriverWait(this.ChromeDriver, new TimeSpan(0, 0, 0, 0, 1500));
        
        // // //
        
        this.Chart = (WebElement)this.WebWait.Until(driver => driver.FindElement(this.Chart_Locator));
    }

    ////  ////  ////

    internal readonly WebDriverWait WebWait;
    private readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1);
    
    private readonly WebElement Chart;

    
    private List<LuxAlgoCandlestick> RegisteredTVCandlesticks = new List<LuxAlgoCandlestick>();
    public LuxAlgoCandlestick[] Candlesticks => this.RegisteredTVCandlesticks.ToArray();

    ////  ////  ////
    
    private async Task WebpageZoomInAsync()
    {
        await Task.Run(() =>
        {
            int width = this.Chart.Size.Width;
            int height = this.Chart.Size.Height;

            this.ChromeDriver.MoveCursorToLocationOnElement(this.Chart, Convert.ToInt32(-0.005 * width), Convert.ToInt32(0.8 * height)); // makes the ZoomInButton visible
            WebElement ZoomInButton = (WebElement)this.WebWait.Until(driver => driver.FindElement(this.ZoomInButton_Locator));
            for (int i = 0; i < 30; i++)
                ZoomInButton.Click();
        });
    }
    internal async Task ExportAndReadChartDataAsync()
    {
        await Task.Run(() =>
        {
            string[] files_before = Directory.GetFiles(this.downloadsDirectory);
            try
            {
                this.WebWait.Until(driver => driver.FindElement(this.ManageLayoutsButton_Locator)).Click();
                this.WebWait.Until(driver => driver.FindElement(this.ExportChartDataButton_Locator)).Click();
                this.WebWait.Until(driver => driver.FindElement(this.ExportChartDataConfirmButton_Locator)).Click();
            }
            catch (Exception)
            {
                throw;
                // this.ChromeDriver.MoveCursorToLocationAndClick(); Thread.Sleep(500); // manage layouts
                // this.ChromeDriver.MoveCursorToLocationAndClick(); Thread.Sleep(500); // export chart data
                // this.ChromeDriver.MoveCursorToLocationAndClick(); Thread.Sleep(500); // export chart data confirmation
            }

            Thread.Sleep(1000);

            string[] files_after = Directory.GetFiles(this.downloadsDirectory);
            string[] new_files = files_after.Where(file_path => !files_before.Contains(file_path)).Where(file_path => file_path.EndsWith(".csv")).ToArray();

            #region Excepted scenarios
            if (new_files.Length == 0)
                throw new IOException("The csv file wasn't downloaded");

            if (new_files.Length > 1)
                throw new IOException($"Multiple csv files appeared in the '{this.downloadsDirectory}' directory and the program can not determine which one contains the tradingview chart data");
            #endregion

            string csv_file_path = new_files[0];

            using (StreamReader reader = new StreamReader(csv_file_path))
            using (CsvReader csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap<CsvTVCandlestickMap>();
                this.RegisteredTVCandlesticks = csv.GetRecords<LuxAlgoCandlestick>().ToList();
                this.RegisteredTVCandlesticks.ForEach(c => c.CurrencyPair = new CurrencyPair("ETH", "BUSD"));
            }

            this.RegisteredTVCandlesticks.RemoveAt(this.RegisteredTVCandlesticks.Count - 1); // the incomplete (current) candlestick gets removed
            File.Delete(csv_file_path);
        });
    }
    
    private LuxAlgoCandlestick DataWindow_text_to_Candlestick(string data_window_text)
    {
        List<string> data_window_lines = data_window_text.Replace("\r\n", "\n").Split('\n').ToList();
        
        #region #region Edit data_window_lines
        string[] desired_strings = new string[] { "Date", "Time", "Open", "Close", "High", "Low", "Buy", "Strong Buy", "Sell", "Strong Sell", "Exit Buy", "Exit Sell" };

        data_window_lines.RemoveAll(str =>
        {
            foreach (string desired_str in desired_strings)
                if (str.StartsWith(desired_str))
                    return false;

            return true;
        });

        foreach (string desired_str in desired_strings)
        {
            int index = data_window_lines.FindIndex(item => item.StartsWith(desired_str)); // find index of desired string in list
            data_window_lines[index] = data_window_lines[index].Replace(desired_str, string.Empty);
        } 
        #endregion

        return new LuxAlgoCandlestick
        {
            CurrencyPair = new CurrencyPair("ETH", "BUSD"),

            Date = DateTime.Parse(data_window_lines[1], CultureInfo.InvariantCulture),

            Open = decimal.Parse(data_window_lines[2], CultureInfo.InvariantCulture),
            High = decimal.Parse(data_window_lines[3], CultureInfo.InvariantCulture),
            Low = decimal.Parse(data_window_lines[4], CultureInfo.InvariantCulture),
            Close = decimal.Parse(data_window_lines[5], CultureInfo.InvariantCulture),

            Buy = decimal.Parse(data_window_lines[6], CultureInfo.InvariantCulture) == decimal.One,
            StrongBuy = decimal.Parse(data_window_lines[7], CultureInfo.InvariantCulture) == decimal.One,
            Sell = decimal.Parse(data_window_lines[8], CultureInfo.InvariantCulture) == decimal.One,
            StrongSell = decimal.Parse(data_window_lines[9], CultureInfo.InvariantCulture) == decimal.One,
            ExitBuy = double.Parse(data_window_lines[10].Replace("∅", "NaN").Replace("n/a", "NaN").Replace("N/A", "NaN"), CultureInfo.InvariantCulture),
            ExitSell = double.Parse(data_window_lines[11].Replace("∅", "NaN").Replace("n/a", "NaN").Replace("N/A", "NaN"), CultureInfo.InvariantCulture)
        };
    }
    private async Task<LuxAlgoCandlestick> GetLastCompleteCandlestickAsync()
    {
        return await Task.Run(() =>
        {
            int maxAttempts = 50;
            for (int i = 0; i < maxAttempts; i++)
            {
                try
                {
                    int width = this.Chart.Size.Width;
                    int height = this.Chart.Size.Height;

                    this.ChromeDriver.MoveCursorToLocationOnElement(this.Chart, Convert.ToInt32(-0.267 * width), Convert.ToInt32(0.128 * height));
                    return this.DataWindow_text_to_Candlestick(this.WebWait.Until(driver => driver.FindElement(this.DataWindow_Locator)).Text);
                }
                catch (Exception)
                {
                    if (i > 0 && i % 5 == 0) // lazy circuit breaker implementation
                        Thread.Sleep(1000);
                }
            }

            #region Build exception message and throw
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"Could not get exeecute method \"{MethodBase.GetCurrentMethod().Name}\"");
            builder.AppendLine($"Exceeded {maxAttempts} attempts");
            throw new Exception(builder.ToString());
            #endregion
        });
    }
    private async Task<LuxAlgoCandlestick> GetUnfinishedCandlestickAsync()
    {
        return await Task.Run(() =>
        {
            int maxAttempts = 50;
            for (int i = 0; i < maxAttempts; i++)
            {
                try
                {
                    int width = this.Chart.Size.Width;
                    int height = this.Chart.Size.Height;

                    this.ChromeDriver.MoveCursorToLocationOnElement(this.Chart, Convert.ToInt32(0.267 * width), Convert.ToInt32(0.128 * height));
                    return this.DataWindow_text_to_Candlestick(this.WebWait.Until(driver => driver.FindElement(this.DataWindow_Locator)).Text);
                }
                catch (Exception)
                {
                    if (i > 0 && i % 5 == 0) // lazy circuit breaker implementation
                        Thread.Sleep(1000);
                }
            }
            
            #region Build exception message and throw
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"Could not get exeecute method \"{MethodBase.GetCurrentMethod().Name}\"");
            builder.AppendLine($"Exceeded {maxAttempts} attempts");
            throw new Exception(builder.ToString());
            #endregion
        });
    }
    
    public async Task<LuxAlgoCandlestick> WaitForNextCandleAsync()
    {
        try
        {
            await this.Semaphore.WaitAsync();
            

            LuxAlgoCandlestick LastCandle = await this.GetUnfinishedCandlestickAsync();
            LuxAlgoCandlestick LastCompleteCandle = await this.GetLastCompleteCandlestickAsync();
            TimeSpan difference = LastCandle.Date - LastCompleteCandle.Date;

            // holds the program here until a new candlestick has been completed
            while (LastCandle.Date - LastCompleteCandle.Date == difference)
                LastCandle = await this.GetUnfinishedCandlestickAsync();

            LastCompleteCandle = await this.GetLastCompleteCandlestickAsync();
            this.RegisteredTVCandlesticks.Add(LastCompleteCandle);
            return LastCompleteCandle;
        }
        finally
        {
            this.Semaphore.Release();
        }
    }
    public async Task<LuxAlgoCandlestick> WaitForNextMatchingCandleAsync(params Predicate<LuxAlgoCandlestick>[] matches)
    {
        bool OneMatches(LuxAlgoCandlestick candle, IEnumerable<Predicate<LuxAlgoCandlestick>> match_arr)
        {
            foreach (Predicate<LuxAlgoCandlestick> match in matches)
                if (match.Invoke(candle))
                    return true;
            return false;
        }

        try
        {
            await this.Semaphore.WaitAsync();


            #region Valid input check
            if (matches is null)
                throw new ArgumentNullException(nameof(matches));

            if (matches.Length == 0)
                throw new ArgumentException($"No predicate was specified for {nameof(matches)}");
            #endregion

            LuxAlgoCandlestick LastCompleteCandle;
            do
            {
                LastCompleteCandle = await this.WaitForNextCandleAsync();
            } while (!OneMatches(LastCompleteCandle, matches));

            return LastCompleteCandle;
        }
        finally
        {
            this.Semaphore.Release();
        }
    }
   
    public async Task<decimal> GetUnfinishedCandlestickOpenPriceAsync() => (await this.GetUnfinishedCandlestickAsync()).Open;
    
    public async Task RegisterAllCandlesticksAsync()
    {
        try
        {
            await this.Semaphore.WaitAsync();
            
            await this.WebpageZoomInAsync();
            await this.ExportAndReadChartDataAsync();
        }
        finally
        {
            this.Semaphore.Release();
        }
    }

    //// //// ////
    
    public void Dispose()
    {
        try
        {
            this.ChromeDriver.Dispose();
            this.Semaphore.Dispose();
            this.RegisteredTVCandlesticks.Clear();
        }
        finally { GC.SuppressFinalize(this); }
    }
}
