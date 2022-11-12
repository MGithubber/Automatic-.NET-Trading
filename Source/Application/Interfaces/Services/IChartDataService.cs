using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Skender.Stock.Indicators;

namespace AutomaticDotNETtrading.Application.Interfaces.Services;

public interface IChartDataService<T> : IDisposable where T : IQuote
{
    /// <summary>
    /// Gets all of the completed candlesticks
    /// </summary>
    public T[] Candlesticks { get; }

    /// <summary>
    /// Starts and returns the task of waiting for the current work in progress candle to be completed
    /// </summary>
    /// <returns></returns>
    public Task<T> WaitForNextCandleAsync();

    /// <summary>
    /// Starts and returns the task of waiting for the conditions defined by one of the specified predicates to be true and returns the matching <see cref="TVCandlestick"/> object instance
    /// </summary>
    /// <param name="matches"></param>
    /// <returns></returns>
    public Task<T> WaitForNextMatchingCandleAsync(params Predicate<T>[] matches);

    /// <summary>
    /// Gets the unfinished candlestick's open price
    /// </summary>
    /// <returns></returns>
    public Task<decimal> GetUnfinishedCandlestickOpenPriceAsync();

    /// <summary>
    /// Registers all candlesticks
    /// </summary>
    /// <returns></returns>
    public Task RegisterAllCandlesticksAsync();

    public void Quit();
}
