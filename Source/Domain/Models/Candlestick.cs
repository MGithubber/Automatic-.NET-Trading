using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Skender.Stock.Indicators;

namespace AutomaticDotNETtrading.Domain.Models;

public class Candlestick : IQuote, ICloneable
{
    public CurrencyPair CurrencyPair { get; set; } = default!;

    public DateTime Date { get; set; }
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; set; }
    public decimal Volume { get; set; }
    

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
