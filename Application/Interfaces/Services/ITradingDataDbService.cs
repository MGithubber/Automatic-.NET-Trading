using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Skender.Stock.Indicators;

namespace AutomaticDotNETtrading.Application.Interfaces.Services;

public interface ITradingDataDbService<TCandlestick> where TCandlestick : IQuote
{
    public bool AddCandlestick(TCandlestick candlestick);
    public int AddCandlesticks(TCandlestick[] candlesticks)
    {
        int ctr = 0;

        for (int i = 0; i < candlesticks.Length; i++)
            if (this.AddCandlestick(candlesticks[i]))
                ctr++;
        
        return ctr;
    }
}
