using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AutomaticDotNETtrading.Application.Interfaces.Data;
using AutomaticDotNETtrading.Infrastructure.Data;
using AutomaticDotNETtrading.Infrastructure.TradingStrategies.LuxAlgoAndPSAR.Enums;
using AutomaticDotNETtrading.Infrastructure.TradingStrategies.LuxAlgoAndPSAR.Models;
using Binance.Net.Enums;
using Binance.Net.Objects.Models.Futures;
using Bogus;

using Respawn;

namespace Infrastructure.Tests.Integration.TradingDataDbServiceTests;

[TestFixture]
public abstract class TradingDataDbServiceTestsFixture
{
    protected const string ConnectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=\"Binance trading logs\";Integrated Security=True";
    protected readonly IDatabaseConnectionFactory<SqlConnection> ConnectionFactory = new SqlDatabaseConnectionFactory(ConnectionString);
    protected readonly ITradingDataDbService<TVCandlestick> SUT = new TradingDataDbService(ConnectionString);
    protected Respawner DbRespawner = default!; // will clear all data in the database when ResetAsync is called


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

            object _ = f.PickRandom<LuxAlgoSignal>() switch
            {
                LuxAlgoSignal.Buy => c.Buy = true,
                LuxAlgoSignal.StrongBuy => c.StrongBuy = true,
                LuxAlgoSignal.Sell => c.Sell = true,
                LuxAlgoSignal.StrongSell => c.StrongSell = true,
                LuxAlgoSignal.ExitBuy => c.ExitBuy = f.Random.Double(950, 3050),
                LuxAlgoSignal.ExitSell => c.ExitSell = f.Random.Double(950, 3050),
                _ => () => { }
                ,
            };
        });

    protected readonly Faker<BinanceFuturesOrder> FuturesOrdersFaker = new Faker<BinanceFuturesOrder>()
        .RuleFor(order => order.Symbol, f => f.Finance.Currency().Code)
        .RuleFor(order => order.Id, f => f.Random.Long(0, long.MaxValue))
        .RuleFor(order => order.CreateTime, f => f.Date.Between(f.Date.Recent(5480), f.Date.Soon(5480)))
        .RuleFor(order => order.Side, f => f.PickRandom<OrderSide>())
        .RuleFor(order => order.Type, f => f.PickRandom<FuturesOrderType>())
        .RuleFor(order => order.Price, f => f.Random.Decimal(1000, 3000))
        .RuleFor(order => order.Quantity, f => f.Random.Decimal(0.01m, 10));

    
    [SetUp]
    public async Task Setup()
    {
        this.DbRespawner = await Respawner.CreateAsync(ConnectionString);
    }
    
    [TearDown]
    public async Task Cleanup()
    {
        await this.DbRespawner.ResetAsync(ConnectionString);
    }
}
