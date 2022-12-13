using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using AutomaticDotNETtrading.Application.Interfaces.Services;
using AutomaticDotNETtrading.Domain.Models;
using AutomaticDotNETtrading.Infrastructure.Internal;

using Binance.Net.Enums;

using CsvHelper;
using CsvHelper.Configuration;

using Microsoft.Playwright;

using Skender.Stock.Indicators;

namespace AutomaticDotNETtrading.Infrastructure.Services;

/// <summary>
/// Represents an immutable object providing methods for extracting chart data from https://www.tradingview.com using google chrome
/// </summary>
public class TradingviewService<TCandlestick> : IChartDataService<TCandlestick> where TCandlestick : IQuote
{
    private readonly SemaphoreSlim Semaphore = new(1);

    private readonly CurrencyPair CurrencyPair;
    private readonly KlineInterval Timeframe;

    private readonly IPlaywright playwright;
    private readonly IBrowserContext browser;
    private readonly IPage page;

    private readonly string downloadsDirectory;
    
    private readonly Func<string, TCandlestick> Converter;
    private readonly ClassMap<TCandlestick> ClassMap;
     
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

    public static async Task<TradingviewService<TCandlestick>> CreateAsync(CurrencyPair currencyPair, KlineInterval timeframe, Func<string, TCandlestick> converter, ClassMap<TCandlestick> classMap, string userDataDirectory, string downloadsDirectory)
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

        return new TradingviewService<TCandlestick>(currencyPair, timeframe, browser, playwright, page, downloadsDirectory, converter, classMap);
    }
    protected internal TradingviewService(CurrencyPair currencyPair, KlineInterval timeframe, IBrowserContext browser, IPlaywright playwright, IPage page, string downloadsDirectory, Func<string, TCandlestick> converter, ClassMap<TCandlestick> classMap)
    {
        this.CurrencyPair = currencyPair;
        this.Timeframe = timeframe;

        this.browser = browser;
        this.playwright = playwright;
        this.page = page;

        this.downloadsDirectory = downloadsDirectory;

        this.Converter = converter;
        this.ClassMap = classMap;

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


    ////  ////  ////


    private List<TCandlestick> RegisteredTVCandlesticks = new();
    public TCandlestick[] Candlesticks => this.RegisteredTVCandlesticks.ToArray();


    private async Task DownloadCsvChartData()
    {
        await this.ManageLayoutsButton_Locator.ClickAsync();
        await this.ExportChartDataButton_Locator.ClickAsync();
        await this.ExportChartDataConfirmButton_Locator.ClickAsync();
    }
    private async Task ZoomInChart(LocatorClickOptions options)
    {
        for (int i = 0; i < 25; i++)
            await this.ZoomInButton_Locator.ClickAsync(options);
    }
    private void RegiserCandlestickFromCsvChartData(string path)
    {
        using StreamReader reader = new StreamReader(path);
        using CsvReader csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        
        csv.Context.RegisterClassMap(this.ClassMap);
        this.RegisteredTVCandlesticks = csv.GetRecords<TCandlestick>().ToList();
    }
    private async Task GotoChart()
    {
        // Selects the chart
        await this.page.Keyboard.TypeAsync(this.CurrencyPair.Name);
        await this.page.Keyboard.PressAsync("ArrowDown");
        await this.page.Keyboard.PressAsync("Enter");

        // Selects the timeframe
        await this.page.Keyboard.TypeAsync(((int)this.Timeframe / 60).ToString());
        await this.page.Keyboard.PressAsync("Enter");
    }
    public async Task RegisterAllCandlesticksAsync()
    {
        using var _ = new LockedOperation(this.Semaphore);

        await this.GotoChart();
        
        await this.ZoomInChart(new LocatorClickOptions { Force = true });
        await this.DownloadCsvChartData();

        string path = Directory.GetFiles(this.downloadsDirectory).First();
        this.RegiserCandlestickFromCsvChartData(path);
        File.Delete(path);
    }
    
    private async Task<TCandlestick> GetLastCompleteCandlestickAsync()
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

                return this.Converter.Invoke(await this.DataWindow_Locator.InnerTextAsync());
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
    private async Task<TCandlestick> GetUnfinishedCandlestickAsync()
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
                
                return this.Converter.Invoke(await this.DataWindow_Locator.InnerTextAsync());
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


    public async Task<TCandlestick> WaitForNextCandleAsync()
    {
        using var _ = new LockedOperation(this.Semaphore);

        TCandlestick LastCandle = await this.GetUnfinishedCandlestickAsync();
        TCandlestick LastCompleteCandle = await this.GetLastCompleteCandlestickAsync();
        TimeSpan difference = LastCandle.Date - LastCompleteCandle.Date;

        // holds the program here until a new candlestick has been completed
        while (LastCandle.Date - LastCompleteCandle.Date == difference)
            LastCandle = await this.GetUnfinishedCandlestickAsync();

        LastCompleteCandle = await this.GetLastCompleteCandlestickAsync();
        this.RegisteredTVCandlesticks.Add(LastCompleteCandle);
        return LastCompleteCandle;
    }
    public async Task<TCandlestick> WaitForNextMatchingCandleAsync(params Predicate<TCandlestick>[] matches)
    {
        using var _ = new LockedOperation(this.Semaphore);

        #region Valid input check
        if (matches is null)
            throw new ArgumentNullException(nameof(matches));

        if (matches.Length == 0)
            throw new ArgumentException($"No predicate was specified for {nameof(matches)}");
        #endregion

        TCandlestick LastCompleteCandle;
        do
        {
            LastCompleteCandle = await this.WaitForNextCandleAsync();
        } while (matches.Any(match => match.Invoke(LastCompleteCandle)));

        return LastCompleteCandle;
    }


    public async Task<decimal> GetUnfinishedCandlestickOpenPriceAsync()
    {
        using var _ = new LockedOperation(this.Semaphore);
        
        TCandlestick candlestick = await this.GetUnfinishedCandlestickAsync();
        return candlestick.Open;
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
