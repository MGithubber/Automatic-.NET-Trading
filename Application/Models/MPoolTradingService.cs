using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using AutomaticDotNETtrading.Application.Interfaces;
using AutomaticDotNETtrading.Application.Interfaces.Data;
using AutomaticDotNETtrading.Application.Interfaces.Services;
using AutomaticDotNETtrading.Application.Interfaces.Services.Internal;
using AutomaticDotNETtrading.Domain.Models;

using Binance.Net.Enums;
using Binance.Net.Objects.Models.Futures;
using Binance.Net.Objects.Models.Spot;

using CryptoExchange.Net.CommonObjects;
using CryptoExchange.Net.Objects;

using Skender.Stock.Indicators;

namespace AutomaticDotNETtrading.Application.Models;

public class MPoolTradingService<TCandlestick, TDatabaseConnection> : IPoolTradingService where TCandlestick : IQuote where TDatabaseConnection : IDbConnection
{
    private readonly IChartDataService<TCandlestick> ChartDataService = default!;
    private readonly ITradingDataDbService<TCandlestick> TradingDataDbService = default!;
    private readonly List<ITradingStrategy<TCandlestick>>? Traders = null;

    public MPoolTradingService(IChartDataService<TCandlestick> chartDataService, ITradingDataDbService<TCandlestick> tradingDataDbService, params ITradingStrategy<TCandlestick>[] traders)
    {
        this.ChartDataService = this.ChartDataService ?? throw new ArgumentNullException(nameof(chartDataService));
        this.TradingDataDbService = this.TradingDataDbService ?? throw new ArgumentNullException(nameof(tradingDataDbService));
        this.Traders = this.Traders is not null ? traders.ToList() : throw new ArgumentNullException(nameof(traders));

        #region Input error checks
        if (traders.Length == 0)
            throw new ArgumentException(nameof(this.Traders));

        IEnumerable<ICfdTradingApiService> contractTraders = this.Traders.Select(t => t.ContractTrader);
        if (contractTraders.Count() != contractTraders.Distinct().Count())
            throw new ArgumentException();
        #endregion

        //// ////
        
        this.OnNewCandlestickRegistered += (object? sender, TCandlestick e) => this.TradingDataDbService.AddCandlestick(e);

        this.Traders.ForEach(trader =>
        {
            trader.OnPositionOpened += Trader_OnPositionOpened;
            trader.OnPositionClosed += Trader_OnPositionClosed;
        });
        void Trader_OnPositionOpened(object? sender, KeyValuePair<TCandlestick, FuturesPosition> e)
        {
            new List<BinanceFuturesOrder> { e.Value.EntryOrder, e.Value.StopLossOrder!, e.Value.TakeProfitOrder! }
            .Where(order => order is not null)
            .ToList()
            .ForEach(order => this.TradingDataDbService.AddFuturesOrder(order, e.Key, out int _, out int _));

            // // TO DO position saving in the database // //
        }
        void Trader_OnPositionClosed(object? sender, KeyValuePair<TCandlestick, BinanceFuturesOrder> e)
        {
            this.TradingDataDbService.AddFuturesOrder(e.Value, e.Key, out int _, out int _);
            
            // // TO DO position saving in the database // //
        }
    }

    //// //// ////

    public event EventHandler<TCandlestick>? OnNewCandlestickRegistered;

    //// ////

    private TCandlestick[] CompletedCandlesticks = default!;
    private decimal LastOpenPrice;
        
    public async Task StartTradingAsync()
    {
        if (this.Traders is null)
            throw new InvalidOperationException($"Attempted to start trading with the when {nameof(this.Traders)} was NULL", new NullReferenceException($"{nameof(this.Traders)} was NULL"));

        // //
        
        await this.ChartDataService.RegisterAllCandlesticksAsync();
        
        while (true)
        {
            await this.ChartDataService.WaitForNextCandleAsync();
            this.CompletedCandlesticks = this.ChartDataService.Candlesticks;
            this.LastOpenPrice = await this.ChartDataService.GetUnfinishedCandlestickOpenPriceAsync();


            this.OnNewCandlestickRegistered?.Invoke(this.ChartDataService, this.CompletedCandlesticks.Last());
            
            this.Traders.ForEach(trader => trader.SendData((TCandlestick[])this.CompletedCandlesticks.Clone(), this.LastOpenPrice));
            try { Parallel.Invoke(this.Traders.Select(trader => new Action(trader.MakeMove)).ToArray()); }
            catch (Exception exception) { /*TO DO exception handling*/ }
            // previously in catch: Console.WriteLine($"==============================\n\nEXCEPTION AT Parallel.Invoke(traders)\n\n{exception}\n\n==============================");
        }
    }
    
    //// ////

    public void QuitChartDataService() => this.ChartDataService.Quit();
}
