using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// // // BINANCE .NET // // //
using Binance.Net.Clients;
using Binance.Net.Clients.UsdFuturesApi;
using Binance.Net.Enums;
using Binance.Net.Interfaces.Clients;
using Binance.Net.Interfaces.Clients.UsdFuturesApi;
using Binance.Net.Objects.Models.Futures;
using Binance.Net.Objects.Models.Spot;

// // // CRYPTO EXCHANGE .NET // // //
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.CommonObjects;
using CryptoExchange.Net.Objects;

using AutomaticDotNETtrading.Domain.Models;
using AutomaticDotNETtrading.Domain.Extensions;
using AutomaticDotNETtrading.Application.Interfaces.Services;

namespace AutomaticDotNETtrading.Infrastructure.Services;

public class BinanceApiService : ICfdTradingApiService
{
    public CurrencyPair CurrencyPair { get; }
    private readonly IBinanceClient BinanceClient;
    private readonly IBinanceClientUsdFuturesApi FuturesClient;
    private readonly IBinanceClientUsdFuturesApiTrading TradingClient;
    private readonly IBinanceClientUsdFuturesApiExchangeData ExchangeData;

    public decimal Leverage { get; }
    
    public BinanceApiService(CurrencyPair CurrencyPair, ApiCredentials ApiCredentials, decimal Leverage = 10)
    {
        this.CurrencyPair = CurrencyPair ?? throw new ArgumentNullException(nameof(CurrencyPair));
        
        this.BinanceClient = new BinanceClient();
        this.BinanceClient.SetApiCredentials(ApiCredentials ?? throw new ArgumentNullException(nameof(ApiCredentials)));
        this.FuturesClient = this.BinanceClient.UsdFuturesApi;
        this.TradingClient = this.BinanceClient.UsdFuturesApi.Trading;
        this.ExchangeData = this.FuturesClient.ExchangeData;

        this.Leverage = Leverage;
    }

    //// //// ////

    public FuturesPosition? Position { get; private set; }
    

    public async Task<CallResult<BinanceFuturesOrder>> GetOrderAsync(long orderID) => await this.TradingClient.GetOrderAsync(symbol: this.CurrencyPair.Name, orderId: orderID);
    
    public async Task<decimal> GetCurrentPriceAsync()
    {
        WebCallResult<BinancePrice> callResult = await this.ExchangeData.GetPriceAsync(this.CurrencyPair.Name);
        if (!callResult.Success)
        {
            throw new Exception($"Could not get the price for {this.CurrencyPair}");
        }
        
        return callResult.Data.Price;
    }
    public async Task<decimal> GetEquityAsync()
    {
        CallResult<BinanceFuturesAccountInfo> callResult = await this.FuturesClient.Account.GetAccountInfoAsync();
        if (!callResult.Success)
            throw new Exception("Could not get the account information");

        BinanceFuturesAccountAsset[] filteredAssets = callResult.Data.Assets.Where(binanceAsset => this.CurrencyPair.Name.EndsWith(binanceAsset.Asset)).ToArray();
        if (filteredAssets.Length == 0)
            throw new Exception($"No assets found for {this.CurrencyPair}");

        else if (filteredAssets.Length > 1)
            throw new Exception($"Multiple margin-assets found for {this.CurrencyPair}");

        return filteredAssets[0].AvailableBalance;
    }

    //// //// ////

    private async Task<(decimal, decimal)> Get_BaseQuantity_and_CurrentPrice_Async(decimal MarginBUSD)
    {
        decimal BaseQuantity;
        decimal equityBUSD;
        decimal CurrentPrice;

        Task<decimal> GetCurrentPrice_Task = this.GetCurrentPriceAsync();
        equityBUSD = (MarginBUSD == decimal.MaxValue) ? await this.GetEquityAsync() : MarginBUSD;
        CurrentPrice = await GetCurrentPrice_Task;

        BaseQuantity = Math.Round(equityBUSD * this.Leverage / CurrentPrice, 2, MidpointRounding.ToZero);

        return (BaseQuantity, CurrentPrice);
    }
    private static ArgumentError? ValidateInput(OrderSide OrderSide, decimal? StopLoss_price, decimal? TakeProfit_price, decimal CurrentPrice)
    {
        StringBuilder builder = new StringBuilder();
        if (StopLoss_price <= 0)
            builder.AppendLine($"Invalid argument for {nameof(StopLoss_price)}, specified value was {StopLoss_price}");

        if (TakeProfit_price <= 0)
            builder.AppendLine($"Invalid argument for {nameof(TakeProfit_price)}, specified value was {TakeProfit_price}");

        if (builder.Length != 0)
            return new ArgumentError(builder.Remove(builder.Length - 1, 1).ToString());

        if (OrderSide == OrderSide.Buy)
        {
            builder = new StringBuilder();

            if (StopLoss_price >= CurrentPrice)
                builder.AppendLine($"The stop loss can't be greater than or equal to the current price for a {OrderSide} order, current price was {CurrentPrice} and stop loss was {StopLoss_price}");

            if (TakeProfit_price <= CurrentPrice)
                builder.AppendLine($"The take profit can't be less greater than or equal to the current price for a {OrderSide} order, current price was {CurrentPrice} and take profit was {TakeProfit_price}");
        }
        else
        {
            builder = new StringBuilder();

            if (StopLoss_price <= CurrentPrice)
                builder.AppendLine($"The stop loss can't be less greater than or equal to the current price for a {OrderSide} order, current price was {CurrentPrice} and stop loss was {StopLoss_price}");


            if (TakeProfit_price >= CurrentPrice)
                builder.AppendLine($"The take profit can't be greater than or equal to the current price for a {OrderSide} order, current price was {CurrentPrice} and take profit was {TakeProfit_price}");
        }

        if (builder.Length != 0)
            return new ArgumentError(builder.Remove(builder.Length - 1, 1).ToString());

        return null;
    }
    private List<BinanceFuturesBatchOrder> CreateBinanceBatchOrders(OrderSide OrderSide, decimal? StopLoss_price, decimal? TakeProfit_price, decimal BaseQuantity)
    {
        List<BinanceFuturesBatchOrder> BatchOrders = new()
        {
            new BinanceFuturesBatchOrder
            {
                Symbol = this.CurrencyPair.Name,
                Side = OrderSide,
                Type = FuturesOrderType.Market,
                Quantity = BaseQuantity,
            }
        };
        if (StopLoss_price.HasValue)
        {
            BatchOrders.Add(new BinanceFuturesBatchOrder
            {
                Symbol = this.CurrencyPair.Name,
                Side = OrderSide.Invert(),
                Type = FuturesOrderType.StopMarket,
                Quantity = BaseQuantity,
                StopPrice = Math.Round(StopLoss_price.Value, 2),
            });
        }
        if (TakeProfit_price.HasValue)
        {
            BatchOrders.Add(new BinanceFuturesBatchOrder
            {
                Symbol = this.CurrencyPair.Name,
                Side = OrderSide.Invert(),
                Type = FuturesOrderType.TakeProfitMarket,
                Quantity = BaseQuantity,
                StopPrice = Math.Round(TakeProfit_price.Value, 2),
            });
        }

        return BatchOrders;
    }
    private FuturesPosition CreateFuturesPosition(CallResult<IEnumerable<CallResult<BinanceFuturesPlacedOrder>>> CallResult, decimal MarginBUSD)
    {
        List<BinanceFuturesPlacedOrder> PlacedOrders = CallResult.Data.Select(call => call.Data).Where(placedOrder => placedOrder is not null).ToList();
        List<BinanceFuturesOrder> FuturesOrders = Enumerable.Range(0, 3).Select(_ => new BinanceFuturesOrder()).ToList();
        Parallel.For(0, PlacedOrders.Count, i => FuturesOrders[i] = this.GetOrderAsync(PlacedOrders[i].Id).GetAwaiter().GetResult().Data); // Parallel.For isn't async await friendly

        return new FuturesPosition(this.CurrencyPair)
        {
            Leverage = this.Leverage,
            Margin = MarginBUSD,

            EntryOrder = FuturesOrders[0],
            StopLossOrder = FuturesOrders[1],
            TakeProfitOrder = FuturesOrders[2]
        };
    }
    public async Task<CallResult<IEnumerable<CallResult<BinanceFuturesPlacedOrder>>>> OpenPositionAtMarketPriceAsync(OrderSide OrderSide, decimal MarginBUSD = decimal.MaxValue, decimal? StopLoss_price = null, decimal? TakeProfit_price = null)
    {
        (decimal BaseQuantity, decimal CurrentPrice) = await this.Get_BaseQuantity_and_CurrentPrice_Async(MarginBUSD);

        ArgumentError? error = ValidateInput(OrderSide, StopLoss_price, TakeProfit_price, CurrentPrice);
        if (error != null)
        {
            return new(error);
        }
                
        List<BinanceFuturesBatchOrder> BatchOrders = this.CreateBinanceBatchOrders(OrderSide, StopLoss_price, TakeProfit_price, BaseQuantity);
        CallResult<IEnumerable<CallResult<BinanceFuturesPlacedOrder>>> CallResult = await this.TradingClient.PlaceMultipleOrdersAsync(orders: BatchOrders.ToArray());
        if (!CallResult.Success)
        {
            return CallResult;
        }
        
        this.Position = this.CreateFuturesPosition(CallResult, MarginBUSD);

        return CallResult;
    }


    public async Task<CallResult<BinanceFuturesOrder>> ClosePositionAsync()
    {
        if (this.Position is null)
        {
            return new(new ArgumentError($"Can't close position since no position is open, {nameof(this.Position)} is NULL"));
        }
        

        CallResult<BinanceFuturesPlacedOrder> ClosingCallResult = await this.TradingClient.PlaceOrderAsync(symbol: this.CurrencyPair.Name, side: this.Position.EntryOrder.Side.Invert(), type: FuturesOrderType.Market, quantity: this.Position.EntryOrder.Quantity);
        if (!ClosingCallResult.Success)
        {
            return ClosingCallResult.As<BinanceFuturesOrder>(null);
        }


        Task<CallResult<BinanceFuturesOrder>> GetFuturesOrderTask = this.GetOrderAsync(ClosingCallResult.Data.Id);
        

        WebCallResult<IEnumerable<CallResult<BinanceFuturesCancelOrder>>> CancellingCallResult = await this.TradingClient.CancelMultipleOrdersAsync(symbol: this.CurrencyPair.Name, this.Position.GetOrdersIDs().ToList());
        if (!CancellingCallResult.Success)
        {
            return ClosingCallResult.As((await GetFuturesOrderTask).Data);
        }
        
        
        this.Position = null;
        return ClosingCallResult.As((await GetFuturesOrderTask).Data);
    }

    public async Task<CallResult<BinanceFuturesPlacedOrder>> PlaceStopLossAsync(decimal price)
    {
        if (this.Position is null)
        {
            return new(new ArgumentError("Can't place a new stop loss order since no position is open"));
        }


        CallResult<BinanceFuturesPlacedOrder> CallResult = await this.TradingClient.PlaceOrderAsync(symbol: this.CurrencyPair.Name, side: this.Position.EntryOrder.Side.Invert(), type: FuturesOrderType.StopMarket, quantity: this.Position.EntryOrder.Quantity, stopPrice: Math.Round(price, 2));
        if (!CallResult.Success)
        {
            return CallResult;
        }
        
        
        Task<CallResult<BinanceFuturesOrder>> GetFuturesOrderTask = this.GetOrderAsync(CallResult.Data.Id);
        if (this.Position.StopLossOrder is not null)
        {
            await this.TradingClient.CancelOrderAsync(symbol: this.CurrencyPair.Name, this.Position.StopLossOrder.Id);
        }

        this.Position.StopLossOrder = (await GetFuturesOrderTask).Data;
        

        return CallResult;
    }


    //// //// ////
    

    public void Dispose()
    {
        try
        {
            this.FuturesClient?.Dispose();
            this.BinanceClient.Dispose();
        }
        finally { GC.SuppressFinalize(this); }
    }
}
