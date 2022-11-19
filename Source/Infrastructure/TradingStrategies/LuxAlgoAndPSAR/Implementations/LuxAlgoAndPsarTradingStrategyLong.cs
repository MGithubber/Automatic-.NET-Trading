using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AutomaticDotNETtrading.Application.Interfaces;
using AutomaticDotNETtrading.Application.Interfaces.Services;
using AutomaticDotNETtrading.Infrastructure.Enums;
using AutomaticDotNETtrading.Infrastructure.Services;
using AutomaticDotNETtrading.Infrastructure.TradingStrategies.LuxAlgoAndPSAR.Enums;
using AutomaticDotNETtrading.Infrastructure.TradingStrategies.LuxAlgoAndPSAR.Models;
using Binance.Net.Enums;

namespace AutomaticDotNETtrading.Infrastructure.TradingStrategies.LuxAlgoAndPSAR.Implementations;

public sealed class LuxAlgoAndPsarTradingStrategyLong : LuxAlgoAndPsarTradingStrategy
{
    public LuxAlgoAndPsarTradingStrategyLong(TradingParameters TradingParams, ICfdTradingApiService ContractTrader) : base(TradingParams, ContractTrader)
    {
        this.TradingParams.ExitSL = decimal.One - this.TradingParams.ExitSL / (decimal)100.0;
        this.TradingParams.ExitBreakeven = decimal.One + this.TradingParams.ExitBreakeven / (decimal)100.0;
        this.TradingParams.AscendingLows_or_DescendingHighs = decimal.One - this.TradingParams.AscendingLows_or_DescendingHighs / (decimal)100.0;
        this.TradingParams.AscendingCloses_or_DescendingCloses = decimal.One - this.TradingParams.AscendingCloses_or_DescendingCloses / (decimal)100.0;
        this.TradingParams.OriginalSL = decimal.One - this.TradingParams.OriginalSL / (decimal)100.0;
        this.TradingParams.WorstCaseSL = decimal.One - this.TradingParams.WorstCaseSL / (decimal)100.0;
        this.TradingParams.Breakeven = decimal.One + this.TradingParams.Breakeven / (decimal)100.0;

        // // //

        this.StoppedOut = false;
        this.EntryPrice = 0;
        this.LastTradedSignal = LuxAlgoSignal.Hold;
        this.StopLoss = 0;
        this.ExitSignalPrice = long.MaxValue;
    }

    //// //// //// ////

    private void TrailingStopLoss(int i)
    {
        if (this.StopLoss < this.EntryPrice)
            if (this.TradingParams.Breakeven * this.EntryPrice < this.LastOpenPrice)
            {
                this.StopLoss = this.EntryPrice;
                this.PlaceNewStopLoss(this.StopLoss).Wait();
            }

        if (this.StopLoss < this.ExitSignalPrice)
            if (this.TradingParams.ExitBreakeven * this.ExitSignalPrice < this.LastOpenPrice)
            {
                this.StopLoss = this.ExitSignalPrice;
                this.PlaceNewStopLoss(this.StopLoss).Wait();
            }

        decimal sl_close = this.LastCandle.Open * this.TradingParams.AscendingCloses_or_DescendingCloses;
        decimal sl_low = this.LastCandle.Low * this.TradingParams.AscendingLows_or_DescendingHighs;

        bool ascending_closes = true;
        bool ascending_lows = true;

        #region conditions determining
        for (int x = 1; x <= 4; x++)
            if (this.Candlesticks[i - x - 1].Close > this.Candlesticks[i - x].Close)
            {
                ascending_closes = false;
                break;
            }

        for (int x = 1; x <= 4; x++)
            if (this.Candlesticks[i - x - 1].Low > this.Candlesticks[i - x].Low)
            {
                ascending_lows = false;
                break;
            }
        #endregion

        #region conditions based trading actions
        if (ascending_closes && this.StopLoss < sl_close && sl_close < this.LastOpenPrice)
        {
            this.StopLoss = Math.Max(this.StopLoss, sl_close);
            this.PlaceNewStopLoss(this.StopLoss).Wait();
        }
        else if (ascending_lows && this.StopLoss < sl_low && sl_low < this.LastOpenPrice)
        {
            this.StopLoss = Math.Max(this.StopLoss, sl_low);
            this.PlaceNewStopLoss(this.StopLoss).Wait();
        }
        #endregion
    }

    private void OpenPosition()
    {
        this.StoppedOut = false;
        this.EntryPrice = this.LastOpenPrice;
        this.StopLoss = Math.Max(this.EntryPrice * this.TradingParams.WorstCaseSL, this.LastCandle.Low * this.TradingParams.OriginalSL);

        this.OpenFuturesPosition(OrderSide.Buy, this.StopLoss).Wait();
    }
    private void ClosePosition(decimal exit_prc)
    {
        this.StoppedOut = false;
        this.EntryPrice = 0;
        this.StopLoss = 0;
        this.ExitSignalPrice = long.MaxValue;

        this.CloseFuturesPosition().Wait();
    }
    private void stop_out_position_long(decimal stop_loss_prc)
    {
        this.StoppedOut = true;
        this.EntryPrice = 0;
        this.StopLoss = 0;
        this.ExitSignalPrice = long.MaxValue;

        this.OnStopOutDetected_Invoke(this, this.LastCandle);
    }

    private bool ParabolicSarDivergence(int i)
    {
        if (this.LastCandle.IsBullish) // checks for divergence if a bullish candle has formed
        {
            decimal[] PsarValues = this.GetParabolicSAR();

            int bullish_candle_index = -1;
            int divergence_length = 0; // nr. candles in the divergence from candles[bullish_candle_index] to candles[i - 2]

            for (int j = i - 2; this.Candlesticks[j].LuxAlgoSignal != this.LastTradedSignal; j--)
            {
                divergence_length++;
                if (this.Candlesticks[j].IsBullish && PsarValues[j] < PsarValues[j + 1])
                {
                    bullish_candle_index = j;
                    break;
                }
            }

            if (bullish_candle_index != -1 && divergence_length > 3)
                if (this.Candlesticks[bullish_candle_index].Close > this.Candlesticks[i - 2].Close && PsarValues[bullish_candle_index] < PsarValues[i - 2])
                {
                    Dictionary<LuxAlgoCandlestick, decimal> dict = new Dictionary<LuxAlgoCandlestick, decimal>();
                    try
                    {
                        List<LuxAlgoCandlestick> divergenceCandles = this.Candlesticks.ToList().GetRange(bullish_candle_index, this.Candlesticks.Length - bullish_candle_index);
                        List<decimal> divergencePsars = PsarValues.ToList().GetRange(bullish_candle_index, this.Candlesticks.Length - bullish_candle_index);

                        for (int divIndex = 0; divIndex < divergenceCandles.Count; divIndex++)
                            dict.Add(divergenceCandles[divIndex], divergencePsars[divIndex]);
                    }
                    catch
                    {
                        dict.Add(this.Candlesticks[bullish_candle_index], PsarValues[bullish_candle_index]);
                        dict.Add(this.LastCandle, PsarValues.Last());
                    }

                    this.OnParabolicSARdivergence_Invoke(this, dict);
                    return true;
                }
        }

        return false;
    }

    public override void MakeMove()
    {
        int i = this.Candlesticks.Length - 1;

        this.GetTrendDirection();

        // // // //

        if (this.StopLoss != 0)
        {
            // old condition: Math.Min(LastCandle.Low, LastOpenPrice) >= StopLoss
            if (!this.IsInPosition())
            {
                this.stop_out_position_long(this.StopLoss);
                goto LuxAlgo_signal_check;
            }

            this.TrailingStopLoss(i);
        }

    LuxAlgo_signal_check:

        if (this.StopLoss == 0) // currently not in position
        {
            if (this.LastCandle.LuxAlgoSignal == LuxAlgoSignal.Buy || this.LastCandle.LuxAlgoSignal == LuxAlgoSignal.StrongBuy || this.LastCandle.LuxAlgoSignal == LuxAlgoSignal.ExitSell)
            {
                this.OpenPosition();
                this.LastTradedSignal = this.LastCandle.LuxAlgoSignal;
            }
            else if (this.TrendDirection == TrendDirection.Uptrend && this.StoppedOut == true && this.ParabolicSarDivergence(i))
            {
                this.OpenPosition();
            }
        }
        else // position is currently open
        {
            if (this.LastCandle.LuxAlgoSignal == LuxAlgoSignal.Sell || this.LastCandle.LuxAlgoSignal == LuxAlgoSignal.StrongSell)
            {
                this.ClosePosition(this.LastOpenPrice);
            }
            else if (this.LastCandle.LuxAlgoSignal == LuxAlgoSignal.ExitBuy)
            {
                this.StopLoss = Math.Max(this.StopLoss, this.LastCandle.Low * this.TradingParams.ExitSL);
                this.PlaceNewStopLoss(this.StopLoss).Wait();

                this.ExitSignalPrice = Math.Max(this.LastCandle.Open, this.LastCandle.Close);
            }
        }
    }
}
