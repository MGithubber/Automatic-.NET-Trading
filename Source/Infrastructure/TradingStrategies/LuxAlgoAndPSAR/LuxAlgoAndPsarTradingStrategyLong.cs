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

public sealed class LuxAlgoAndPsarTradingStrategyLong : LuxAlgoAndPsarTradingStrategy
{
    public LuxAlgoAndPsarTradingStrategyLong(TradingParameters TradingParams, BinanceCfdTradingApiService ContractTrader) : base(TradingParams, ContractTrader)
    {
        this.TradingParams.ExitSL = decimal.One - this.TradingParams.ExitSL / (decimal)100.0;
        this.TradingParams.ExitBreakeven = decimal.One + this.TradingParams.ExitBreakeven / (decimal)100.0;
        this.TradingParams.AscendingLows_or_DescendingHighs = decimal.One - this.TradingParams.AscendingLows_or_DescendingHighs / (decimal)100.0;
        this.TradingParams.AscendingCloses_or_DescendingCloses = decimal.One - this.TradingParams.AscendingCloses_or_DescendingCloses / (decimal)100.0;
        this.TradingParams.OriginalSL = decimal.One - this.TradingParams.OriginalSL / (decimal)100.0;
        this.TradingParams.WorstCaseSL = decimal.One - this.TradingParams.WorstCaseSL / (decimal)100.0;
        this.TradingParams.Breakeven = decimal.One + this.TradingParams.Breakeven / (decimal)100.0;

        // // //

        StoppedOut = false;
        EntryPrice = 0;
        LastTradedSignal = LuxAlgoSignal.Hold;
        StopLoss = 0;
        ExitSignalPrice = long.MaxValue;
    }

    //// //// //// ////
    
    private void TrailingStopLoss(int i)
    {
        if (StopLoss < EntryPrice)
            if (TradingParams.Breakeven * EntryPrice < LastOpenPrice)
            {
                StopLoss = EntryPrice;
                base.PlaceNewStopLoss(StopLoss).Wait();
            }

        if (StopLoss < ExitSignalPrice)
            if (TradingParams.ExitBreakeven * ExitSignalPrice < LastOpenPrice)
            {
                StopLoss = ExitSignalPrice;
                base.PlaceNewStopLoss(StopLoss).Wait();
            }

        decimal sl_close = LastCandle.Open * TradingParams.AscendingCloses_or_DescendingCloses;
        decimal sl_low = LastCandle.Low * TradingParams.AscendingLows_or_DescendingHighs;

        bool ascending_closes = true;
        bool ascending_lows = true;

        #region conditions determining
        for (int x = 1; x <= 4; x++)
            if (Candlesticks[i - x - 1].Close > Candlesticks[i - x].Close)
            {
                ascending_closes = false;
                break;
            }

        for (int x = 1; x <= 4; x++)
            if (Candlesticks[i - x - 1].Low > Candlesticks[i - x].Low)
            {
                ascending_lows = false;
                break;
            }
        #endregion

        #region conditions based trading actions
        if (ascending_closes && StopLoss < sl_close && sl_close < LastOpenPrice)
        {
            StopLoss = Math.Max(StopLoss, sl_close);
            base.PlaceNewStopLoss(StopLoss).Wait();
        }
        else if (ascending_lows && StopLoss < sl_low && sl_low < LastOpenPrice)
        {
            StopLoss = Math.Max(StopLoss, sl_low);
            base.PlaceNewStopLoss(StopLoss).Wait();
        }
        #endregion
    }

    private void OpenPosition()
    {
        StoppedOut = false;
        EntryPrice = LastOpenPrice;
        StopLoss = Math.Max(EntryPrice * TradingParams.WorstCaseSL, LastCandle.Low * TradingParams.OriginalSL);

        base.OpenFuturesPosition(OrderSide.Buy, StopLoss).Wait();
    }
    private void ClosePosition(decimal exit_prc)
    {
        StoppedOut = false;
        EntryPrice = 0;
        StopLoss = 0;
        ExitSignalPrice = long.MaxValue;

        base.CloseFuturesPosition().Wait();
    }
    private void stop_out_position_long(decimal stop_loss_prc)
    {
        StoppedOut = true;
        EntryPrice = 0;
        StopLoss = 0;
        ExitSignalPrice = long.MaxValue;

        base.OnStopOutDetected_Invoke(this, LastCandle);
    }

    private bool ParabolicSarDivergence(int i)
    {
        if (LastCandle.IsBullish) // checks for divergence if a bullish candle has formed
        {
            decimal[] PsarValues = GetParabolicSAR();

            int bullish_candle_index = -1;
            int divergence_length = 0; // nr. candles in the divergence from candles[bullish_candle_index] to candles[i - 2]

            for (int j = i - 2; Candlesticks[j].LuxAlgoSignal != LastTradedSignal; j--)
            {
                divergence_length++;
                if (Candlesticks[j].IsBullish && PsarValues[j] < PsarValues[j + 1])
                {
                    bullish_candle_index = j;
                    break;
                }
            }

            if (bullish_candle_index != -1 && divergence_length > 3)
                if (Candlesticks[bullish_candle_index].Close > Candlesticks[i - 2].Close && PsarValues[bullish_candle_index] < PsarValues[i - 2])
                {
                    Dictionary<TVCandlestick, decimal> dict = new Dictionary<TVCandlestick, decimal>();
                    try
                    {
                        List<TVCandlestick> divergenceCandles = Candlesticks.ToList().GetRange(bullish_candle_index, Candlesticks.Length - bullish_candle_index);
                        List<decimal> divergencePsars = PsarValues.ToList().GetRange(bullish_candle_index, Candlesticks.Length - bullish_candle_index);

                        for (int divIndex = 0; divIndex < divergenceCandles.Count; divIndex++)
                            dict.Add(divergenceCandles[divIndex], divergencePsars[divIndex]);
                    }
                    catch
                    {
                        dict.Add(Candlesticks[bullish_candle_index], PsarValues[bullish_candle_index]);
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
            // old condition: Math.Min(LastCandle.Low, LastOpenPrice) >= StopLoss
            if (!IsInPosition())
            {
                stop_out_position_long(StopLoss);
                goto LuxAlgo_signal_check;
            }

            TrailingStopLoss(i);
        }

    LuxAlgo_signal_check:

        if (StopLoss == 0) // currently not in position
        {
            if (LastCandle.LuxAlgoSignal == LuxAlgoSignal.Buy || LastCandle.LuxAlgoSignal == LuxAlgoSignal.StrongBuy || LastCandle.LuxAlgoSignal == LuxAlgoSignal.ExitSell)
            {
                OpenPosition();
                LastTradedSignal = LastCandle.LuxAlgoSignal;
            }
            else if (TrendDirection == TrendDirection.Uptrend && StoppedOut == true && ParabolicSarDivergence(i))
            {
                OpenPosition();
            }
        }
        else // position is currently open
        {
            if (LastCandle.LuxAlgoSignal == LuxAlgoSignal.Sell || LastCandle.LuxAlgoSignal == LuxAlgoSignal.StrongSell)
            {
                ClosePosition(LastOpenPrice);
            }
            else if (LastCandle.LuxAlgoSignal == LuxAlgoSignal.ExitBuy)
            {
                StopLoss = Math.Max(StopLoss, LastCandle.Low * TradingParams.ExitSL);
                base.PlaceNewStopLoss(StopLoss).Wait();

                ExitSignalPrice = Math.Max(LastCandle.Open, LastCandle.Close);
            }
        }
    }
}
