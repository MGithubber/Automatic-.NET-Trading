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
    /// <param name="Candlestick">The candlestick to be added</param>
    /// <returns></returns>
    public Task<int> AddCandlestickAsync(TCandlestick Candlestick);

    /// <summary>
    /// Deletes a <see cref="TCandlestick"/> synchronously from the database then returns its database identity
    /// </summary>
    /// <param name="Candlestick">The candlestick to be deleted</param>
    /// <returns></returns>
    public Task<int> DeleteCandlestickAsync(TCandlestick Candlestick);


    /// <summary>
    /// Adds a <see cref="BinanceFuturesOrder"/> along with its corresponding <see cref="TCandlestick"/> synchronously to the database then returns their database identities
    /// </summary>
    /// <param name="FuturesOrder">The order to be added</param>
    /// <param name="Candlestick">The candlestick to be either added or located</param>
    /// <param name="FuturesOrder_Id">The order database identity after it has been added</param>
    /// <param name="Candlestick_Id">The candlestick database identity after it has been added or located</param>
    public Task<(int FuturesOrder_Id, int Candlestick_Id)> AddFuturesOrderAsync(BinanceFuturesOrder FuturesOrder, TCandlestick Candlestick);

    /// <summary>
    /// Deletes a <see cref="BinanceFuturesOrder"/> synchronously from the database then returns its database identity
    /// </summary>
    /// <param name="FuturesOrder">The order to be deleted</param>
    /// <returns></returns>
    public Task<int> DeleteFuturesOrderAsync(BinanceFuturesOrder FuturesOrder);
}
