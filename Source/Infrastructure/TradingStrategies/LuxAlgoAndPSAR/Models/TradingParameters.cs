using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomaticDotNETtrading.Infrastructure.TradingStrategies.LuxAlgoAndPSAR.Models;

/// <summary>
/// <para>Represents an object containing the trading parameters necessary for the trading method</para>
/// </summary>
public class TradingParameters : ICloneable
{
    /// <summary>
    /// <para>Used while in position</para>
    /// <para>Updates the trailing stop loss if the last completed candle has a LuxAlgo exit signal indicating that the current trend could end</para>
    /// <para>stop_loss = max(stop_loss, candles[i - 1].low * <see cref="ExitSL"/>);</para>
    /// <para>exit_signal_price = max(candles[i - 1].open, candles[i - 1].close);</para>
    /// </summary>
    public decimal ExitSL { get; set; }

    /// <summary>
    /// <para>Used while in position</para>
    /// <para>Will be used to place the stop loss at the exit_signal_price (at LuxAlgo exit signal)</para>
    /// </summary>
    public decimal ExitBreakeven { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public decimal AscendingLows_or_DescendingHighs { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public decimal AscendingCloses_or_DescendingCloses { get; set; }

    /// <summary>
    /// <para>Used when opening a new position in pair with <see cref="WorstCaseSL"/></para>
    /// <para>stop_loss = max(entry_price * <see cref="WorstCaseSL"/>, candles[i - 1].low * <see cref="OriginalSL"/>);</para>
    /// </summary>
    public decimal OriginalSL { get; set; }

    /// <summary>
    /// <para>Used when opening a new position in pair with <see cref="OriginalSL"/></para>
    /// <para>stop_loss = max(entry_price * <see cref="WorstCaseSL"/>, candles[i - 1].low * <see cref="OriginalSL"/>);</para>
    /// </summary>
    public decimal WorstCaseSL { get; set; }

    /// <summary>
    /// Used while in position to determine weather to place the stop loss at the entry price or not
    /// </summary>
    public decimal Breakeven { get; set; }

    //// //// ////

    public object Clone() => MemberwiseClone();
}
