﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AutomaticDotNETtrading.Application.Interfaces;
using AutomaticDotNETtrading.Application.Interfaces.Services;
using AutomaticDotNETtrading.Infrastructure.Enums;
using AutomaticDotNETtrading.Infrastructure.Models;
using AutomaticDotNETtrading.Infrastructure.Services;
using Binance.Net.Enums;
using Binance.Net.Objects.Models.Futures;
using CryptoExchange.Net.Objects;
using Skender.Stock.Indicators;

namespace AutomaticDotNETtrading.Infrastructure.TradingStrategies.LuxAlgoAndPSAR;

public abstract class LuxAlgoAndPsarTradingStrategy : ITradingStrategy<TVCandlestick>
{
    public ICfdTradingApiService ContractTrader { get; }
    public TradingParameters TradingParams { get; }

    public LuxAlgoAndPsarTradingStrategy(TradingParameters TradingParams, BinanceCfdTradingApiService ContractTrader)
    {
        this.TradingParams = TradingParams;
        this.ContractTrader = ContractTrader;
    }

    //// //// //// ////

    #region Events
    public event EventHandler<KeyValuePair<TVCandlestick, IEnumerable<BinanceFuturesPlacedOrder>>>? OnPositionOpened;
    public event EventHandler<KeyValuePair<TVCandlestick, BinanceFuturesPlacedOrder>>? OnStopLossUpdated;
    public event EventHandler<TVCandlestick>? OnStopOutDetected;
    public event EventHandler<KeyValuePair<TVCandlestick, BinanceFuturesPlacedOrder>>? OnPositionClosed;
    public event EventHandler<Dictionary<TVCandlestick, decimal>>? OnParabolicSARdivergence;

    protected void OnPositionOpened_Invoke(object sender, KeyValuePair<TVCandlestick, IEnumerable<BinanceFuturesPlacedOrder>> e) => this.OnPositionOpened?.Invoke(sender, e);
    protected void OnStopLossUpdated_Invoke(object sender, KeyValuePair<TVCandlestick, BinanceFuturesPlacedOrder> e) => this.OnStopLossUpdated?.Invoke(sender, e);
    protected void OnStopOutDetected_Invoke(object sender, TVCandlestick e) => this.OnStopOutDetected?.Invoke(sender, e);
    protected void OnPositionClosed_Invoke(object sender, KeyValuePair<TVCandlestick, BinanceFuturesPlacedOrder> e) => this.OnPositionClosed?.Invoke(sender, e);
    protected void OnParabolicSARdivergence_Invoke(object sender, Dictionary<TVCandlestick, decimal> e) => this.OnParabolicSARdivergence?.Invoke(sender, e);
    #endregion

    //// //// ////
    
    protected TVCandlestick[] Candlesticks = default!;
    protected TVCandlestick LastCandle = default!;
    protected decimal LastOpenPrice;
    protected TrendDirection TrendDirection;

    // Position status related members
    protected bool StoppedOut;
    protected decimal EntryPrice;
    protected LuxAlgoSignal LastTradedSignal;
    protected decimal StopLoss;
    protected decimal ExitSignalPrice;

    protected void GetTrendDirection()
    {
        LuxAlgoSignal signal = LastCandle.LuxAlgoSignal;

        if (signal == LuxAlgoSignal.Buy || signal == LuxAlgoSignal.StrongBuy)
            TrendDirection = TrendDirection.Uptrend;
        else if (signal == LuxAlgoSignal.Sell || signal == LuxAlgoSignal.StrongSell)
            TrendDirection = TrendDirection.Downtrend;
    }

    protected decimal[] GetParabolicSAR() => this.Candlesticks.GetParabolicSar().Select(res => res.Sar.HasValue ? Convert.ToDecimal(res.Sar.Value) : decimal.Zero).ToArray();


    #region IFuturesPairTradingApiService calls
    protected async Task OpenFuturesPosition(OrderSide OrderSide, decimal? StopLoss_price = null, decimal? TakeProfit_price = null)
    {
        CallResult<IEnumerable<CallResult<BinanceFuturesPlacedOrder>>> CallResult = await this.ContractTrader.OpenPositionAtMarketPriceAsync(OrderSide, decimal.MaxValue, StopLoss_price, TakeProfit_price);

        if (!CallResult.Success)
            throw new Exception($"Failed to open a futures order position on {nameof(OrderSide)} {OrderSide}");

        this.OnPositionOpened_Invoke(this, new KeyValuePair<TVCandlestick, IEnumerable<BinanceFuturesPlacedOrder>>(this.LastCandle, CallResult.Data.Select(orderCall => orderCall.Data)));
    }
    protected async Task CloseFuturesPosition()
    {
        CallResult<BinanceFuturesPlacedOrder> CallResult = await this.ContractTrader.ClosePositionAsync();

        if (!CallResult.Success)
            throw new Exception($"Failed to close a futures order position");

        this.OnPositionClosed_Invoke(this, new KeyValuePair<TVCandlestick, BinanceFuturesPlacedOrder>(this.LastCandle, CallResult.Data));
    }
    protected async Task PlaceNewStopLoss(decimal price)
    {
        CallResult<BinanceFuturesPlacedOrder> CallResult = await this.ContractTrader.PlaceStopLossAsync(price);
        
        if (!CallResult.Success)
            throw new Exception($"Failed to place a futures stop loss order");

        this.OnPositionClosed_Invoke(this, new KeyValuePair<TVCandlestick, BinanceFuturesPlacedOrder>(this.LastCandle, CallResult.Data));
    }

    protected bool IsInPosition() => this.ContractTrader.IsInPosition();
    #endregion

    
    public virtual void SendData(TVCandlestick[] Candlesticks, decimal LastOpenPrice)
    {
        this.Candlesticks = Candlesticks;
        this.LastCandle = Candlesticks.Last();
        this.LastOpenPrice = LastOpenPrice;
    }
    public abstract void MakeMove();
}