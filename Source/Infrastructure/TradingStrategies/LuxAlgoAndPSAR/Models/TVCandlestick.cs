using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

// // // INDICATORS .NET // // //
using Skender.Stock.Indicators;
using AutomaticDotNETtrading.Domain.Models;
using AutomaticDotNETtrading.Infrastructure.TradingStrategies.LuxAlgoAndPSAR.Enums;

namespace AutomaticDotNETtrading.Infrastructure.TradingStrategies.LuxAlgoAndPSAR.Models;

/// <summary>
/// Represents a mutable object that contains the data window elements for a candlestick from https://www.tradingview.com
/// </summary>
public class TVCandlestick : Candlestick
{
    // LuxAlgo indicator values
    public bool Buy { get; set; } = false;
    public bool StrongBuy { get; set; } = false;
    public bool Sell { get; set; } = false;
    public bool StrongSell { get; set; } = false;
    public double ExitBuy { get; set; } = double.NaN;
    public double ExitSell { get; set; } = double.NaN;
    public LuxAlgoSignal LuxAlgoSignal
    {
        get
        {
            PropertyInfo[] properties = typeof(TVCandlestick).GetProperties().Where(prop => Enum.IsDefined(typeof(LuxAlgoSignal), prop.Name)).ToArray();
            foreach (PropertyInfo property in properties)
                if (Enum.TryParse(property.Name, out LuxAlgoSignal signal))
                {
                    if (property.PropertyType == typeof(bool)) // confirmation signals (Buy, StrongBuy, Sell, StrongSell)
                        if ((bool)property.GetValue(this)! == true)
                            return signal;

                    if (property.PropertyType == typeof(double)) // exit signals (ExitBuy, ExitSell)
                        if (!double.IsNaN((double)property.GetValue(this)!))
                            return signal;
                }

            return LuxAlgoSignal.Hold;
        }
    }
}
