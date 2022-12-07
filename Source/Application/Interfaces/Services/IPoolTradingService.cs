using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Skender.Stock.Indicators;

namespace AutomaticDotNETtrading.Application.Interfaces.Services;

public interface IPoolTradingService : IDisposable
{
    public int NrTradingStrategies { get; }
    
    public Task StartTradingAsync();
}
