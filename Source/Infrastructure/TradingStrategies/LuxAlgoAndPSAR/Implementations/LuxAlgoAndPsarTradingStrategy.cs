using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AutomaticDotNETtrading.Application.Interfaces;
using AutomaticDotNETtrading.Application.Interfaces.Services;
using AutomaticDotNETtrading.Domain.Models;
using AutomaticDotNETtrading.Infrastructure.Enums;
using AutomaticDotNETtrading.Infrastructure.Services;
using AutomaticDotNETtrading.Infrastructure.TradingStrategies.LuxAlgoAndPSAR.Enums;
using AutomaticDotNETtrading.Infrastructure.TradingStrategies.LuxAlgoAndPSAR.Models;
using Binance.Net.Enums;
using Binance.Net.Objects.Models.Futures;
using CryptoExchange.Net.Objects;
using Skender.Stock.Indicators;

namespace AutomaticDotNETtrading.Infrastructure.TradingStrategies.LuxAlgoAndPSAR.Implementations;

public abstract class LuxAlgoAndPsarTradingStrategy : ITradingStrategy<LuxAlgoCandlestick>
{
    public ICfdTradingApiService ContractTrader { get; }
    public TradingParameters TradingParams { get; }
    
    public LuxAlgoAndPsarTradingStrategy(TradingParameters TradingParams, ICfdTradingApiService ContractTrader)
    {
        this.TradingParams = TradingParams ?? throw new ArgumentNullException(nameof(TradingParams), $"The {nameof(TradingParameters)} value was NULL when initialising an object of type {nameof(LuxAlgoAndPsarTradingStrategy)}");
        this.ContractTrader = ContractTrader ?? throw new ArgumentNullException(nameof(ContractTrader), $"The {nameof(ICfdTradingApiService)} value was NULL when initialising an object of type {nameof(LuxAlgoAndPsarTradingStrategy)}");
    }
    
    //// //// //// ////

    #region Events
    public event EventHandler<KeyValuePair<LuxAlgoCandlestick, FuturesPosition>>? OnPositionOpened;
    public event EventHandler<KeyValuePair<LuxAlgoCandlestick, BinanceFuturesPlacedOrder>>? OnStopLossUpdated;
    public event EventHandler<LuxAlgoCandlestick>? OnStopOutDetected;
    public event EventHandler<KeyValuePair<LuxAlgoCandlestick, BinanceFuturesOrder>>? OnPositionClosed;
    public event EventHandler<Dictionary<LuxAlgoCandlestick, decimal>>? OnParabolicSARdivergence;
    
    protected void OnPositionOpened_Invoke(object sender, KeyValuePair<LuxAlgoCandlestick, FuturesPosition> e) => this.OnPositionOpened?.Invoke(sender, e);
    protected void OnStopLossUpdated_Invoke(object sender, KeyValuePair<LuxAlgoCandlestick, BinanceFuturesPlacedOrder> e) => this.OnStopLossUpdated?.Invoke(sender, e);
    protected void OnStopOutDetected_Invoke(object sender, LuxAlgoCandlestick e) => this.OnStopOutDetected?.Invoke(sender, e);
    protected void OnPositionClosed_Invoke(object sender, KeyValuePair<LuxAlgoCandlestick, BinanceFuturesOrder> e) => this.OnPositionClosed?.Invoke(sender, e);
    protected void OnParabolicSARdivergence_Invoke(object sender, Dictionary<LuxAlgoCandlestick, decimal> e) => this.OnParabolicSARdivergence?.Invoke(sender, e);
    #endregion

    //// //// ////
    
    protected internal LuxAlgoCandlestick[] Candlesticks = default!;
    protected internal LuxAlgoCandlestick LastCandle = default!;
    protected internal decimal LastOpenPrice;
    protected internal TrendDirection TrendDirection;
    
    // Position status related members
    protected internal bool StoppedOut;
    protected internal decimal EntryPrice;
    protected internal LuxAlgoSignal LastTradedSignal;
    protected internal decimal StopLoss;
    protected internal decimal ExitSignalPrice;


    #region Market related methods
    protected void GetTrendDirection()
    {
        LuxAlgoSignal signal = this.LastCandle.LuxAlgoSignal;
        
        if (signal == LuxAlgoSignal.Buy || signal == LuxAlgoSignal.StrongBuy)
            this.TrendDirection = TrendDirection.Uptrend;

        else if (signal == LuxAlgoSignal.Sell || signal == LuxAlgoSignal.StrongSell)
            this.TrendDirection = TrendDirection.Downtrend;
    }
    protected decimal[] GetParabolicSAR() => this.Candlesticks.GetParabolicSar().Select(res => res.Sar.HasValue ? Convert.ToDecimal(res.Sar.Value) : decimal.Zero).ToArray(); 
    #endregion

    #region IFuturesPairTradingApiService calls
    protected async Task OpenFuturesPosition(OrderSide OrderSide, decimal? StopLoss_price = null, decimal? TakeProfit_price = null)
    {
        await this.ContractTrader.OpenPositionAtMarketPriceAsync(OrderSide, decimal.MaxValue, StopLoss_price, TakeProfit_price);
        
        _ = this.ContractTrader.Position ?? throw new Exception($"Failed to open a futures order position on {nameof(OrderSide)} {OrderSide}", new NullReferenceException(nameof(this.ContractTrader.Position)));

        this.OnPositionOpened_Invoke(this, new KeyValuePair<LuxAlgoCandlestick, FuturesPosition>(this.LastCandle, this.ContractTrader.Position));
    }
    protected async Task CloseFuturesPosition()
    {
        CallResult<BinanceFuturesOrder> CallResult = await this.ContractTrader.ClosePositionAsync();
        
        if (this.ContractTrader.Position is not null)
            throw new Exception($"Failed to close a futures order position");

        this.OnPositionClosed_Invoke(this, new KeyValuePair<LuxAlgoCandlestick, BinanceFuturesOrder>(this.LastCandle, CallResult.Data));
    }
    protected async Task PlaceNewStopLoss(decimal price)
    {
        await this.ContractTrader.PlaceStopLossAsync(price);
        
        _ = this.ContractTrader.Position ?? throw new NullReferenceException($"{nameof(this.ContractTrader.Position)} was NULL");
        _ = this.ContractTrader.Position.StopLossOrder ?? throw new Exception($"Failed to place a futures stop loss order", new NullReferenceException(nameof(this.ContractTrader.Position)));

        this.OnPositionClosed_Invoke(this, new KeyValuePair<LuxAlgoCandlestick, BinanceFuturesOrder>(this.LastCandle, this.ContractTrader.Position.StopLossOrder));
    }

    protected bool IsInPosition() => this.ContractTrader.IsInPosition();
    #endregion


    public virtual void SendData(LuxAlgoCandlestick[] Candlesticks, decimal LastOpenPrice)
    {
        this.Candlesticks = Candlesticks;
        this.LastCandle = Candlesticks.Last();
        this.LastOpenPrice = LastOpenPrice;
    }
    public abstract void MakeMove();


    //// //// ////
    
    
    public void Dispose()
    {
        try { this.ContractTrader.Dispose(); }
        finally { GC.SuppressFinalize(this); }
    }
}
