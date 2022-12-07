using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AutomaticDotNETtrading.Application.Interfaces;
using AutomaticDotNETtrading.Infrastructure.TradingStrategies.LuxAlgoAndPSAR.Implementations;
using AutomaticDotNETtrading.Infrastructure.TradingStrategies.LuxAlgoAndPSAR.Models;
using Bogus;

namespace Infrastructure.Tests.Unit.LuxAlgoAndPsarTradingStrategyTests;

public abstract class LuxAlgoAndPsarTradingStrategyTestsBase
{
    protected readonly ICfdTradingApiService BinaceApi = Substitute.For<ICfdTradingApiService>();
    protected LuxAlgoAndPsarTradingStrategy SUT = default!;
    
    protected readonly Faker<LuxAlgoCandlestick> CandlesticksFaker = new Faker<LuxAlgoCandlestick>()
        .RuleFor(c => c.CurrencyPair, f => new CurrencyPair(f.Finance.Currency().Code, f.Finance.Currency().Code))
        .RuleFor(c => c.Date, f => f.Date.Between(f.Date.Recent(5480), f.Date.Soon(5480)))
        .RuleFor(c => c.Open, f => f.Random.Decimal(1000, 3000))
        .RuleFor(c => c.High, f => f.Random.Decimal(1000, 3000))
        .RuleFor(c => c.Low, f => f.Random.Decimal(1000, 3000))
        .RuleFor(c => c.Close, f => f.Random.Decimal(1000, 3000));



    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        try
        {
            this.BinaceApi.Dispose();
            this.SUT.Dispose();
        }
        finally
        {
             GC.SuppressFinalize(this);
        }
    }
}
