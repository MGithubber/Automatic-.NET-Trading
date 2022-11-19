using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomaticDotNETtrading.Infrastructure.TradingStrategies.LuxAlgoAndPSAR.Enums;
using AutomaticDotNETtrading.Infrastructure.TradingStrategies.LuxAlgoAndPSAR.Models;
using Bogus;

namespace Infrastructure.Tests.Integration.TradingviewChartDataServiceTests;

[TestFixture]
public abstract class TradingviewChartDataServiceTestsFixture
{
    protected IChartDataService<TVCandlestick> SUT = new TradingviewChartDataService();
    
    protected readonly Faker<TVCandlestick> CandlesticksFaker = new Faker<TVCandlestick>()
    .RuleFor(c => c.CurrencyPair, f => new CurrencyPair(f.Finance.Currency().Code, f.Finance.Currency().Code))
    .RuleFor(c => c.Date, f => f.Date.Between(f.Date.Recent(5480), f.Date.Soon(5480)))
    .RuleFor(c => c.Open, f => f.Random.Decimal(1000, 3000))
    .RuleFor(c => c.High, f => f.Random.Decimal(1000, 3000))
    .RuleFor(c => c.Low, f => f.Random.Decimal(1000, 3000))
    .RuleFor(c => c.Close, f => f.Random.Decimal(1000, 3000))
    .Rules((f, c) =>
    {
        if (f.PickRandom(true, false))
            return; // signal stays Hold

        switch (f.PickRandom<LuxAlgoSignal>())
        {
            case LuxAlgoSignal.Buy: c.Buy = true; break;
            case LuxAlgoSignal.StrongBuy: c.StrongBuy = true; break;
            case LuxAlgoSignal.Sell: c.Sell = true; break;
            case LuxAlgoSignal.StrongSell: c.StrongSell = true; break;
            case LuxAlgoSignal.ExitBuy: c.ExitBuy = f.Random.Double(950, 3050); break;
            case LuxAlgoSignal.ExitSell: c.ExitSell = f.Random.Double(950, 3050); break;
        }
    });
}
