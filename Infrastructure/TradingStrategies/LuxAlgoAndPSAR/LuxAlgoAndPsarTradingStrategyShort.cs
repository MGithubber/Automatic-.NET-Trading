using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AutomaticDotNETtrading.Application.Interfaces;
using AutomaticDotNETtrading.Infrastructure.Enums;
using AutomaticDotNETtrading.Infrastructure.Models;
using AutomaticDotNETtrading.Infrastructure.Services;
using Binance.Net.Enums;

namespace AutomaticDotNETtrading.Infrastructure.TradingStrategies.LuxAlgoAndPSAR;

public sealed class LuxAlgoAndPsarTradingStrategyShort : LuxAlgoAndPsarTradingStrategy
{
    public LuxAlgoAndPsarTradingStrategyShort(TradingParameters TradingParams, BinanceCfdTradingApiService ContractTrader) : base(TradingParams, ContractTrader)
    {
        this.TradingParams.ExitSL = decimal.One + this.TradingParams.ExitSL / (decimal)100.0;
        this.TradingParams.ExitBreakeven = decimal.One - this.TradingParams.ExitBreakeven / (decimal)100.0;
        this.TradingParams.AscendingLows_or_DescendingHighs = decimal.One + this.TradingParams.AscendingLows_or_DescendingHighs / (decimal)100.0;
        this.TradingParams.AscendingCloses_or_DescendingCloses = decimal.One + this.TradingParams.AscendingCloses_or_DescendingCloses / (decimal)100.0;
        this.TradingParams.OriginalSL = decimal.One + this.TradingParams.OriginalSL / (decimal)100.0;
        this.TradingParams.WorstCaseSL = decimal.One + this.TradingParams.WorstCaseSL / (decimal)100.0;
        this.TradingParams.Breakeven = decimal.One - this.TradingParams.Breakeven / (decimal)100.0;

        // // //

        StoppedOut = false;
        EntryPrice = 0;
        LastTradedSignal = LuxAlgoSignal.Hold;
        StopLoss = 0;
        ExitSignalPrice = long.MinValue;
    }
    
    //// //// //// ////

    private void TrailingStopLoss(int i)
    {
        if (StopLoss > EntryPrice)
            if (TradingParams.Breakeven * EntryPrice > LastOpenPrice)
            {
                StopLoss = EntryPrice;
                base.PlaceNewStopLoss(StopLoss).Wait();
            }

        if (StopLoss > ExitSignalPrice)
            if (TradingParams.ExitBreakeven * ExitSignalPrice > LastOpenPrice)
            {
                StopLoss = ExitSignalPrice;
                base.PlaceNewStopLoss(StopLoss).Wait();
            }

        decimal sl_close = LastCandle.Open * TradingParams.AscendingCloses_or_DescendingCloses;
        decimal sl_high = LastCandle.High * TradingParams.AscendingLows_or_DescendingHighs;

        bool descending_closes = true;
        bool descending_highs = true;

        #region conditions determining
        for (int x = 1; x <= 4; x++)
            if (Candlesticks[i - x - 1].Close < Candlesticks[i - x].Close)
            {
                descending_closes = false;
                break;
            }

        for (int x = 1; x <= 4; x++)
            if (Candlesticks[i - x - 1].High < Candlesticks[i - x].High)
            {
                descending_highs = false;
                break;
            }
        #endregion

        #region conditions based trading actions
        if (descending_closes && StopLoss > sl_close && sl_close > LastOpenPrice)
        {
            StopLoss = Math.Min(StopLoss, sl_close);
            base.PlaceNewStopLoss(StopLoss).Wait();
        }
        else if (descending_highs && StopLoss > sl_high && sl_high > LastOpenPrice)
        {
            StopLoss = Math.Min(StopLoss, sl_high);
            base.PlaceNewStopLoss(StopLoss).Wait();
        }
        #endregion
    }

    private void OpenPosition()
    {
        StoppedOut = false;
        EntryPrice = LastOpenPrice;
        StopLoss = Math.Min(EntryPrice * TradingParams.WorstCaseSL, LastCandle.High * TradingParams.OriginalSL);

        base.OpenFuturesPosition(OrderSide.Sell, StopLoss).Wait();
    }
    private void ClosePosition(decimal exit_prc)
    {
        StoppedOut = false;
        EntryPrice = 0;
        StopLoss = 0;
        ExitSignalPrice = long.MinValue;

        base.CloseFuturesPosition().Wait();
    }
    private void stop_out_position_short(decimal stop_loss_prc)
    {
        StoppedOut = true;
        EntryPrice = 0;
        StopLoss = 0;
        ExitSignalPrice = long.MinValue;
        
        base.OnStopOutDetected_Invoke(this, LastCandle);
    }

    private bool ParabolicSarDivergence(int i)
    {
        if (LastCandle.IsBearish) // checks for divergence if a bearish candle has formed
        {
            decimal[] PsarValues = GetParabolicSAR();

            int bearish_candle_index = -1;
            int divergence_length = 0; // nr. candles in the divergence from candles[bearish_candle_index] to candles[i - 2]

            for (int j = i - 2; Candlesticks[j].LuxAlgoSignal != LastTradedSignal; j--)
            {
                divergence_length++;
                if (Candlesticks[j].IsBearish && PsarValues[j] > PsarValues[j + 1])
                {
                    bearish_candle_index = j;
                    break;
                }
            }

            if (bearish_candle_index != -1 && divergence_length > 3)
                if (Candlesticks[bearish_candle_index].Close < Candlesticks[i - 2].Close && PsarValues[bearish_candle_index] > PsarValues[i - 2])
                {
                    Dictionary<TVCandlestick, decimal> dict = new Dictionary<TVCandlestick, decimal>();
                    try
                    {
                        List<TVCandlestick> divergenceCandles = Candlesticks.ToList().GetRange(bearish_candle_index, Candlesticks.Length - bearish_candle_index);
                        List<decimal> divergencePsars = PsarValues.ToList().GetRange(bearish_candle_index, Candlesticks.Length - bearish_candle_index);

                        for (int divIndex = 0; divIndex < divergenceCandles.Count; divIndex++)
                            dict.Add(divergenceCandles[divIndex], divergencePsars[divIndex]);
                    }
                    catch
                    {
                        dict.Add(Candlesticks[bearish_candle_index], PsarValues[bearish_candle_index]);
                        dict.Add(LastCandle, PsarValues.Last());
                    }

                    OnParabolicSARdivergence_Invoke(this, dict);
                    return true;
                }
        }

        return false;
    }

    public override void MakeMove()
    {
        int i = Candlesticks.Length - 1;

        GetTrendDirection();

        // // // //

        if (StopLoss != 0)
        {
            // old condition: Math.Max(LastCandle.High, LastOpenPrice) >= StopLoss
            if (!IsInPosition())
            {
                stop_out_position_short(StopLoss);
                goto LuxAlgo_signal_check;
            }

            TrailingStopLoss(i);
        }

    LuxAlgo_signal_check:

        if (StopLoss == 0)  // not in position
        {
            if (LastCandle.LuxAlgoSignal == LuxAlgoSignal.Sell || LastCandle.LuxAlgoSignal == LuxAlgoSignal.StrongSell || LastCandle.LuxAlgoSignal == LuxAlgoSignal.ExitBuy)
            {
                OpenPosition();
                LastTradedSignal = LastCandle.LuxAlgoSignal;
            }
            else if (TrendDirection == TrendDirection.Downtrend && StoppedOut == true && ParabolicSarDivergence(i))
            {
                OpenPosition();
            }
        }
        else // position open
        {
            if (LastCandle.LuxAlgoSignal == LuxAlgoSignal.Buy || LastCandle.LuxAlgoSignal == LuxAlgoSignal.StrongBuy)
            {
                ClosePosition(LastOpenPrice);
            }
            else if (LastCandle.LuxAlgoSignal == LuxAlgoSignal.ExitSell)
            {
                StopLoss = Math.Min(StopLoss, LastCandle.High * TradingParams.ExitSL);
                base.PlaceNewStopLoss(StopLoss).Wait();

                ExitSignalPrice = Math.Min(LastCandle.Open, LastCandle.Close);
            }
        }
    }
}
