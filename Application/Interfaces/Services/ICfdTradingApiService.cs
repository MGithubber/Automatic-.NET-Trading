using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AutomaticDotNETtrading.Domain.Models;

using Binance.Net.Enums;
using Binance.Net.Objects.Models.Futures;

using CryptoExchange.Net.Objects;

namespace AutomaticDotNETtrading.Application.Interfaces.Services;

public interface ICfdTradingApiService
{
    public CurrencyPair CurrencyPair { get; }
    
    public decimal Leverage { get; }

    public FuturesPosition? Position { get; }

    /////  /////  /////
    
    #region public CallResult<BinanceFuturesOrder> GetOrder(...)
    public Task<CallResult<BinanceFuturesOrder>> GetOrderAsync(BinanceFuturesOrder order) => this.GetOrderAsync(order.Id);
    public Task<CallResult<BinanceFuturesOrder>> GetOrderAsync(BinanceFuturesPlacedOrder placedOrder) => this.GetOrderAsync(placedOrder.Id);
    public Task<CallResult<BinanceFuturesOrder>> GetOrderAsync(long orderID);
    #endregion
    
    public Task<decimal> GetCurrentPriceAsync();
    public Task<decimal> GetEquityAsync();
    
    #region Binance orders placing
    public Task<CallResult<IEnumerable<CallResult<BinanceFuturesPlacedOrder>>>> OpenPositionAtMarketPriceAsync(OrderSide OrderSide, decimal MarginBUSD = decimal.MaxValue, decimal? StopLoss_price = null, decimal? TakeProfit_price = null);
    public Task<CallResult<BinanceFuturesPlacedOrder>> ClosePositionAsync();
    public Task<CallResult<BinanceFuturesPlacedOrder>> PlaceStopLossAsync(decimal price);
    #endregion

    public bool IsInPosition() => this.Position is not null;
}
