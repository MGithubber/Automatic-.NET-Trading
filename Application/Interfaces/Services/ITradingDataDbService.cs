using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AutomaticDotNETtrading.Domain.Models;

using Binance.Net.Objects.Models.Futures;

using Skender.Stock.Indicators;

namespace AutomaticDotNETtrading.Application.Interfaces.Services;

public interface ITradingDataDbService<TCandlestick> where TCandlestick : IQuote
{
    /// <summary>
    /// Adds a <see cref="TCandlestick"/> synchronously to the database then returns its database identity
    /// </summary>
    /// <param name="candlestick"></param>
    /// <returns></returns>
    public int AddCandlestick(TCandlestick candlestick);

    /// <summary>
    /// Adds a <see cref="BinanceFuturesOrder"/> along with its corresponding <see cref="TCandlestick"/> synchronously to the database then returns their database identities
    /// </summary>
    /// <param name="candlestick"></param>
    /// <returns></returns>
    public void AddFuturesOrder(BinanceFuturesOrder FuturesOrder, TCandlestick Candlestick, out int FuturesOrder_Id, out int Candlestick_Id);
}
