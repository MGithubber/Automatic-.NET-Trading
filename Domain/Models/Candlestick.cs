using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Skender.Stock.Indicators;

namespace AutomaticDotNETtrading.Domain.Models;

public class Candlestick : IQuote, ICloneable
{
    public CurrencyPair CurrencyPair { get; init; } = default!;

    public DateTime Date { get; init; }
    public decimal Open { get; init; }
    public decimal High { get; init; }
    public decimal Low { get; init; }
    public decimal Close { get; init; }
    public decimal Volume { get; init; }
    

    // Directional information
    public bool IsBullish => Close > Open;
    public bool IsBearish => Close < Open;
    public bool IsDoji => Close == Open;

    public object Clone()
    {
        return new Candlestick
        {
            CurrencyPair = (CurrencyPair)this.CurrencyPair.Clone(),
            
            Date = this.Date,
            Open = this.Open,
            High = this.High,
            Low = this.Low,
            Close = this.Close,
            Volume = this.Volume,
        };
    }
}
