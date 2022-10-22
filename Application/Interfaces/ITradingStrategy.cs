using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomaticDotNETtrading.Application.Interfaces.Services;
using Binance.Net.Objects.Models.Futures;

using CryptoExchange.Net.Objects;

using Skender.Stock.Indicators;

namespace AutomaticDotNETtrading.Application.Interfaces;

public interface ITradingStrategy<TCandlestick> where TCandlestick : IQuote
{
    public ICfdTradingApiService ContractTrader { get; }
    
    public event EventHandler<KeyValuePair<TCandlestick, IEnumerable<BinanceFuturesPlacedOrder>>> OnPositionOpened;
    public event EventHandler<KeyValuePair<TCandlestick, BinanceFuturesPlacedOrder>> OnStopLossUpdated;
    public event EventHandler<TCandlestick> OnStopOutDetected;
    public event EventHandler<KeyValuePair<TCandlestick, BinanceFuturesPlacedOrder>> OnPositionClosed;
    
    public void SendData(TCandlestick[] Candlesticks, decimal LastOpenPrice);
    public void MakeMove();
}
