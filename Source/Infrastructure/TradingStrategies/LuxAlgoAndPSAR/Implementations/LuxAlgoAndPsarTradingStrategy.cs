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
    public event EventHandler<KeyValuePair<TVCandlestick, FuturesPosition>>? OnPositionOpened;
    public event EventHandler<KeyValuePair<TVCandlestick, BinanceFuturesPlacedOrder>>? OnStopLossUpdated;
    public event EventHandler<TVCandlestick>? OnStopOutDetected;
    public event EventHandler<KeyValuePair<TVCandlestick, BinanceFuturesOrder>>? OnPositionClosed;
    public event EventHandler<Dictionary<TVCandlestick, decimal>>? OnParabolicSARdivergence;

    protected void OnPositionOpened_Invoke(object sender, KeyValuePair<TVCandlestick, FuturesPosition> e) => OnPositionOpened?.Invoke(sender, e);
    protected void OnStopLossUpdated_Invoke(object sender, KeyValuePair<TVCandlestick, BinanceFuturesPlacedOrder> e) => OnStopLossUpdated?.Invoke(sender, e);
    protected void OnStopOutDetected_Invoke(object sender, TVCandlestick e) => OnStopOutDetected?.Invoke(sender, e);
    protected void OnPositionClosed_Invoke(object sender, KeyValuePair<TVCandlestick, BinanceFuturesOrder> e) => OnPositionClosed?.Invoke(sender, e);
    protected void OnParabolicSARdivergence_Invoke(object sender, Dictionary<TVCandlestick, decimal> e) => OnParabolicSARdivergence?.Invoke(sender, e);
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

    protected decimal[] GetParabolicSAR() => Candlesticks.GetParabolicSar().Select(res => res.Sar.HasValue ? Convert.ToDecimal(res.Sar.Value) : decimal.Zero).ToArray();


    #region IFuturesPairTradingApiService calls
    protected async Task OpenFuturesPosition(OrderSide OrderSide, decimal? StopLoss_price = null, decimal? TakeProfit_price = null)
    {
        CallResult<IEnumerable<CallResult<BinanceFuturesPlacedOrder>>> CallResult = await ContractTrader.OpenPositionAtMarketPriceAsync(OrderSide, decimal.MaxValue, StopLoss_price, TakeProfit_price);

        if (ContractTrader.Position is null)
            throw new Exception($"Failed to open a futures order position on {nameof(OrderSide)} {OrderSide}", new NullReferenceException(nameof(ContractTrader.Position)));

        OnPositionOpened_Invoke(this, new KeyValuePair<TVCandlestick, FuturesPosition>(LastCandle, ContractTrader.Position));
    }
    protected async Task CloseFuturesPosition()
    {
        CallResult<BinanceFuturesOrder> CallResult = await ContractTrader.ClosePositionAsync();

        if (!CallResult.Success)
            throw new Exception($"Failed to close a futures order position");

        OnPositionClosed_Invoke(this, new KeyValuePair<TVCandlestick, BinanceFuturesOrder>(LastCandle, CallResult.Data));
    }
    protected async Task PlaceNewStopLoss(decimal price)
    {
        CallResult<BinanceFuturesPlacedOrder> CallResult = await ContractTrader.PlaceStopLossAsync(price);

        _ = ContractTrader.Position ?? throw new NullReferenceException($"{nameof(ContractTrader.Position)} was NULL");
        if (ContractTrader.Position.StopLossOrder is null)
            throw new Exception($"Failed to place a futures stop loss order", new NullReferenceException(nameof(ContractTrader.Position)));

        OnPositionClosed_Invoke(this, new KeyValuePair<TVCandlestick, BinanceFuturesOrder>(LastCandle, ContractTrader.Position.StopLossOrder));
    }

    protected bool IsInPosition() => ContractTrader.IsInPosition();
    #endregion


    public virtual void SendData(TVCandlestick[] Candlesticks, decimal LastOpenPrice)
    {
        this.Candlesticks = Candlesticks;
        LastCandle = Candlesticks.Last();
        this.LastOpenPrice = LastOpenPrice;
    }
    public abstract void MakeMove();
}
