using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AutomaticDotNETtrading.Domain.Models;

namespace AutomaticDotNETtrading.Infrastructure.TradingStrategies.TrendlinesWithBreaks.Models;

public class LuxBreaksCandlestick : Candlestick
{
    public required decimal Upper { get; init; }
    public required decimal Lower { get; init; }
    
    public bool UpperBreak => this.IsBullish && this.Close > this.Upper;
    public bool LowerBreak => this.IsBearish && this.Close < this.Lower;
}
