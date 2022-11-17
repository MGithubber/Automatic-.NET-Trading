using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Binance.Net.Enums;
using Binance.Net.Objects.Models.Futures;

namespace AutomaticDotNETtrading.Domain.Models;

public class FuturesPosition
{
    public CurrencyPair CurrencyPair { get; } = default!;

    public DateTime CreateTime => this.EntryOrder.CreateTime;

    public PositionSide Side => this.EntryOrder.Side == OrderSide.Buy ? PositionSide.Long : PositionSide.Short;
    
    public decimal Leverage { get; init; }
    public decimal Margin { get; init; }
    
    protected BinanceFuturesOrder[] OrdersBatch = new BinanceFuturesOrder[3] { null!, null!, null! };
    public BinanceFuturesOrder EntryOrder
    {
        get => this.OrdersBatch[0];
        init
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value), $"The {nameof(this.EntryOrder)} can't be initialised with a NULL value");
            
            #region Incompatible binance futures orders
            if (value.Symbol != this.CurrencyPair)
                throw new ArgumentException($"The {nameof(this.StopLossOrder)} member was given a value with a diffrent {typeof(CurrencyPair).Name}", nameof(this.StopLossOrder));

            if (value.Type != FuturesOrderType.Market)
                throw new ArgumentException($"The {nameof(this.StopLossOrder)} member was given a {value.Type} instead of a market order", nameof(this.StopLossOrder));
            #endregion

            this.OrdersBatch[0] = value;
        }
    }
    public BinanceFuturesOrder? StopLossOrder
    {
        get => this.OrdersBatch[1];
        set
        {
            if(value is null)
            {
                this.OrdersBatch[1] = value!;
                return;
            }
            
            #region Incompatible binance futures orders
            if (value.Symbol != this.CurrencyPair)
                throw new ArgumentException($"The {nameof(this.StopLossOrder)} member was given a value with a diffrent {typeof(CurrencyPair).Name}", nameof(this.StopLossOrder));

            if (value.Type == FuturesOrderType.Market)
                throw new ArgumentException($"The {nameof(this.StopLossOrder)} member was given a market order", nameof(this.StopLossOrder)); 
            #endregion

            this.OrdersBatch[1] = value;
        }
    }
    public BinanceFuturesOrder? TakeProfitOrder
    {
        get => this.OrdersBatch[2];
        set
        {
            if (value is null)
            {
                this.OrdersBatch[2] = value!;
                return;
            }

            #region Incompatible binance futures orders
            if (value.Symbol != this.CurrencyPair)
                throw new ArgumentException($"The {nameof(this.StopLossOrder)} member was given a value with a diffrent {typeof(CurrencyPair).Name}", nameof(this.StopLossOrder));

            if (value.Type == FuturesOrderType.Market)
                throw new ArgumentException($"The {nameof(this.StopLossOrder)} member was given a market order", nameof(this.StopLossOrder)); 
            #endregion

            this.OrdersBatch[2] = value;
        }
    }
    
    public IEnumerable<long> GetOrdersIDs()
    {
        foreach (BinanceFuturesOrder item in this.OrdersBatch)
            yield return item.Id;
    }
    
    #region Position prices of interest getters
    public decimal EntryPrice => this.EntryOrder.AvgPrice;
    public decimal? StopLossPrice => this.StopLossOrder?.StopPrice;
    public decimal? TakeProfitPrice => this.TakeProfitOrder?.StopPrice;
    #endregion

    #region Constructors
    public FuturesPosition(CurrencyPair CurrencyPair) => this.CurrencyPair = CurrencyPair;
    #endregion
}
