using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Skender.Stock.Indicators;

namespace AutomaticDotNETtrading.Domain.Models;

public class Candlestick : IQuote, ICloneable
{
    public required CurrencyPair CurrencyPair { get; init; } = default!;

    public required DateTime Date { get; init; }
    public required decimal Open { get; init; }
    public required decimal High { get; init; }
    public required decimal Low { get; init; }
    public required decimal Close { get; init; }
    public required decimal Volume { get; init; }
    

    // Directional information
    public bool IsBullish => this.Close > this.Open;
    public bool IsBearish => this.Close < this.Open;
    public bool IsDoji => this.Close == this.Open;


    public virtual object Clone()
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
