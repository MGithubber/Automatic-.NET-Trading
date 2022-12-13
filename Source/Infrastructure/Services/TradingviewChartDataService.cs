using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using AutomaticDotNETtrading.Application.Interfaces.Services;
using AutomaticDotNETtrading.Domain.Models;
using AutomaticDotNETtrading.Infrastructure.TradingStrategies.LuxAlgoAndPSAR.Models;

using CsvHelper;

using Microsoft.Playwright;

namespace AutomaticDotNETtrading.Infrastructure.Services;

/// <summary>
/// Represents an immutable object providing methods for extracting chart data from https://www.tradingview.com using google chrome
/// </summary>
public class TradingviewChartDataService : IChartDataService<LuxAlgoCandlestick>
{
    private readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1);

    private IPlaywright playwright;
    private IBrowserContext browser;

    private string downloadsDirectory;
    
    #region Locators
    private readonly ILocator DataWindow_Locator;
    private readonly ILocator Chart_Locator;

    private readonly ILocator ZoomInButton_Locator;
    private readonly ILocator ZoomOutButton_Locator;
    private readonly ILocator ScrollLeftButton_Locator;
    private readonly ILocator ScrollRightButton_Locator;
    private readonly ILocator ResetChartButton_Locator;

    private readonly ILocator ManageLayoutsButton_Locator;
    private readonly ILocator ExportChartDataButton_Locator;
    private readonly ILocator ExportChartDataConfirmButton_Locator;
    #endregion

    protected internal TradingviewChartDataService(IBrowserContext browser, IPlaywright playwright, IPage page, string downloadsDirectory)
    {
        this.browser = browser;
        this.playwright = playwright;
        this.downloadsDirectory = downloadsDirectory;
        
        #region Locators
        this.DataWindow_Locator = page.Locator(".chart-data-window");
        this.Chart_Locator = page.Locator(".chart-gui-wrapper").First;

        this.ZoomInButton_Locator = page.Locator(".control-bar__btn--zoom-in");
        this.ZoomOutButton_Locator = page.Locator(".control-bar__btn--zoom-out");
        this.ScrollLeftButton_Locator = page.Locator(".control-bar__btn--move-left");
        this.ScrollRightButton_Locator = page.Locator(".control-bar__btn--move-right");
        this.ResetChartButton_Locator = page.Locator(".control-bar__btn--turn-button");

        this.ManageLayoutsButton_Locator = page.Locator(".js-save-load-menu-open-button").First;
        this.ExportChartDataButton_Locator = page.GetByText("Export chart data").First;
        this.ExportChartDataConfirmButton_Locator = page.GetByText("Export").Nth(1);
        #endregion
    }
    public static async Task<TradingviewChartDataService> CreateAsync(string userDataDirectory, string downloadsDirectory)
    {
        var browserLaunchOptions = new BrowserTypeLaunchPersistentContextOptions
        {
            Channel = "chrome",
            Headless = false,
            SlowMo = 300,
            AcceptDownloads = true,
            DownloadsPath = downloadsDirectory
        };
        
        var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchPersistentContextAsync(userDataDirectory, browserLaunchOptions);

        IPage page = await browser.NewPageAsync();
        await page.GotoAsync("https://www.tradingview.com/chart/oxqzhJn4/?symbol=BINANCE%3AETHBUSD");

        return new TradingviewChartDataService(browser, playwright, page, downloadsDirectory);
    }

    ////  ////  ////

    private List<LuxAlgoCandlestick> RegisteredTVCandlesticks = new List<LuxAlgoCandlestick>();
    public LuxAlgoCandlestick[] Candlesticks => this.RegisteredTVCandlesticks.ToArray();

    ////  ////  ////
    
    private LuxAlgoCandlestick TextToCandlestick(string text)
    {
        List<string> data_window_lines = text.Replace("\r\n", "\n").Split('\n').ToList();
        List<string> desired_strings = new List<string>() { "Date", "Time", "Open", "Close", "High", "Low", "Buy", "Strong Buy", "Sell", "Strong Sell", "Exit Buy", "Exit Sell" };

        data_window_lines.RemoveAll(line => !desired_strings.Any(desired => line.StartsWith(desired)));
        desired_strings.ToList().ForEach(desired_str =>
        {
            int index = data_window_lines.FindIndex(item => item.StartsWith(desired_str)); // find index of desired string in list
            data_window_lines[index] = data_window_lines[index].Replace(desired_str, string.Empty);
        });

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
        int maxAttempts = 50;
        for (int i = 0; i < maxAttempts; i++)
        {
            try
            {
                var boundingBox = await this.Chart_Locator.BoundingBoxAsync();
                await this.Chart_Locator.HoverAsync(new LocatorHoverOptions
                {
                    Position = new Position
                    {
                        X = (float)(0.33 * boundingBox!.Width),
                        Y = (float)(0.5 * boundingBox!.Height)
                    }
                });

                return TextToCandlestick(await this.DataWindow_Locator.InnerTextAsync());
            }
            catch
            {
                if (i > 0 && i % 5 == 0) // lazy circuit breaker implementation
                    Thread.Sleep(1000);
            }
        }

        #region Build exception message and throw
        StringBuilder builder = new StringBuilder();
        builder.AppendLine($"Could not get exeecute method \"{MethodBase.GetCurrentMethod()!.Name}\"");
        builder.AppendLine($"Exceeded {maxAttempts} attempts");
        throw new Exception(builder.ToString());
        #endregion
    }
    private async Task<LuxAlgoCandlestick> GetUnfinishedCandlestickAsync()
    {
        int maxAttempts = 50;
        for (int i = 0; i < maxAttempts; i++)
        {
            try
            {
                var boundingBox = await this.Chart_Locator.BoundingBoxAsync();
                await this.Chart_Locator.HoverAsync(new LocatorHoverOptions
                {
                    Position = new Position
                    {
                        X = (float)(0.66 * boundingBox!.Width),
                        Y = (float)(0.5 * boundingBox!.Height)
                    }
                });

                return TextToCandlestick(await this.DataWindow_Locator.InnerTextAsync());
            }
            catch
            {
                if (i > 0 && i % 5 == 0) // lazy circuit breaker implementation
                    Thread.Sleep(1000);
            }
        }

        #region Build exception message and throw
        StringBuilder builder = new StringBuilder();
        builder.AppendLine($"Could not get exeecute method \"{MethodBase.GetCurrentMethod()!.Name}\"");
        builder.AppendLine($"Exceeded {maxAttempts} attempts");
        throw new Exception(builder.ToString());
        #endregion
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

            var options = new LocatorClickOptions { Force = true };
            
            for (int i = 0; i < 25; i++) // max zoom
                await this.ZoomInButton_Locator.ClickAsync(options);

            #region Download the .csv file
            await this.ManageLayoutsButton_Locator.ClickAsync(options);
            await this.ExportChartDataButton_Locator.ClickAsync(options);
            await this.ExportChartDataConfirmButton_Locator.ClickAsync(options);
            #endregion

            #region Read the .csv file
            string path = Directory.GetFiles(this.downloadsDirectory).First();
            using (StreamReader reader = new StreamReader(path))
            using (CsvReader csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap<LuxAlgoCandlestickMap>();
                this.RegisteredTVCandlesticks = csv.GetRecords<LuxAlgoCandlestick>().ToList();
            }
            File.Delete(path);
            #endregion
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
            this.Semaphore.Dispose();

            this.browser.DisposeAsync().GetAwaiter().GetResult();
            this.playwright.Dispose();

            this.RegisteredTVCandlesticks.Clear();
        }
        finally { GC.SuppressFinalize(this); }
    }
}
