using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomaticDotNETtrading.Application.Interfaces.Services;
using AutomaticDotNETtrading.Domain.Models;

using Binance.Net.Objects.Models.Futures;

using Skender.Stock.Indicators;

namespace AutomaticDotNETtrading.Application.Interfaces;

public interface ITradingStrategy<TCandlestick> : IDisposable where TCandlestick : IQuote
{
    public ICfdTradingApiService ContractTrader { get; }
    
    public event EventHandler<KeyValuePair<TCandlestick, FuturesPosition>> OnPositionOpened;
    public event EventHandler<KeyValuePair<TCandlestick, BinanceFuturesOrder>> OnPositionClosed;
    
    public void SendData(TCandlestick[] Candlesticks, decimal LastOpenPrice);
    public void MakeMove();
}
