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
/// Represents a mutable object that contains that inherits <see cref="Candlestick"/> and provides a LuxAlgo signal
/// </summary>
public class LuxAlgoCandlestick : Candlestick
{
    // LuxAlgo indicator values
    public required bool Buy { get; set; } = false;
    public required bool StrongBuy { get; set; } = false;
    public required bool Sell { get; set; } = false;
    public required bool StrongSell { get; set; } = false;
    public required double ExitBuy { get; set; } = double.NaN;
    public required double ExitSell { get; set; } = double.NaN;
    public LuxAlgoSignal LuxAlgoSignal
    {
        get
        {
            PropertyInfo[] properties = typeof(LuxAlgoCandlestick).GetProperties().Where(prop => Enum.IsDefined(typeof(LuxAlgoSignal), prop.Name)).ToArray();
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

    
    public override object Clone()
    {
        return new LuxAlgoCandlestick
        {
            CurrencyPair = (CurrencyPair)this.CurrencyPair.Clone(),

            Date = this.Date,
            Open = this.Open,
            High = this.High,
            Low = this.Low,
            Close = this.Close,
            Volume = this.Volume,

            Buy = this.Buy,
            StrongBuy = this.StrongBuy,
            Sell = this.Sell,
            StrongSell = this.StrongSell,
            ExitBuy = this.ExitBuy,
            ExitSell = this.ExitSell
        };
    }
}
