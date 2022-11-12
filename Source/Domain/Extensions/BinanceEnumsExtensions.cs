using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Binance.Net.Enums;

namespace AutomaticDotNETtrading.Domain.Extensions;

public static class BinanceEnumsExtensions
{
    public static OrderSide Invert(this OrderSide orderSide) => orderSide == OrderSide.Buy ? OrderSide.Sell : OrderSide.Buy;
}
