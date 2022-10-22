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
using Binance.Net.Enums;
using Binance.Net.Objects.Models.Futures;
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

        this.Traders.ForEach(trader =>
        {
            trader.OnPositionOpened += Trader_OnPositionOpened;
            trader.OnStopLossUpdated += Trader_OnStopLossUpdated;
            trader.OnStopOutDetected += Trader_OnStopOutDetected;
            trader.OnPositionClosed += Trader_OnPositionClosed;
        });

        void Trader_OnPositionOpened(object? sender, KeyValuePair<TCandlestick, IEnumerable<BinanceFuturesPlacedOrder>> e)
        {
            throw new NotImplementedException();
            // this.OnAnyTraderPositionOpened?.Invoke(sender, e);
        }
        void Trader_OnStopLossUpdated(object? sender, KeyValuePair<TCandlestick, BinanceFuturesPlacedOrder> e)
        {
            throw new NotImplementedException();
            // this.OnAnyTraderStopLossUpdated?.Invoke(sender, e);
        }
        void Trader_OnStopOutDetected(object? sender, TCandlestick e)
        {
            throw new NotImplementedException();
            // this.OnAnyTraderStopOutDetected?.Invoke(sender, e);
        }
        void Trader_OnPositionClosed(object? sender, KeyValuePair<TCandlestick, BinanceFuturesPlacedOrder> e)
        {
            throw new NotImplementedException();
            // this.OnAnyTraderPositionClosed?.Invoke(sender, e);
        }
    }

    //// //// //// ////

    #region Public events
    public event EventHandler<TCandlestick>? OnNewCandlestickRegistered;
    #endregion

    //// //// ////

    private TCandlestick[] CompletedCandlesticks = default!;
    private decimal LastOpenPrice;


    public async Task StartTradingAsync()
    {
        if (this.Traders is null)
            return;


        await this.ChartDataService.RegisterAllCandlesticksAsync();

        while (true)
        {
            await this.ChartDataService.WaitForNextCandleAsync();
            this.CompletedCandlesticks = this.ChartDataService.Candlesticks;
            this.LastOpenPrice = await this.ChartDataService.GetUnfinishedCandlestickOpenPriceAsync();


            this.OnNewCandlestickRegistered?.Invoke(this.ChartDataService, this.CompletedCandlesticks.Last());

            this.Traders.ForEach(trader => trader.SendData((TCandlestick[])this.CompletedCandlesticks.Clone(), this.LastOpenPrice));
            try { Parallel.Invoke(this.Traders.Select(trader => new Action(trader.MakeMove)).ToArray()); }
            catch (Exception exception) { Console.WriteLine($"==============================\n\nEXCEPTION AT Parallel.Invoke(traders)\n\n{exception}\n\n=============================="); }
        }
    }
    
    //// //// ////

    public void QuitChartDataService() => this.ChartDataService.Quit();
}
