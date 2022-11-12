using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Skender.Stock.Indicators;

namespace AutomaticDotNETtrading.Application.Interfaces.Services.Internal;

internal interface IPoolTradingService// <TCandlestick, TDatabaseConnection> where TCandlestick : IQuote where TDatabaseConnection : IDbConnection
{
    public Task StartTradingAsync();
}
