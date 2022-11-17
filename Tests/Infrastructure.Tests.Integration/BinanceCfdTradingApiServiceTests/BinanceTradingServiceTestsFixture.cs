using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Tests.Integration.BinanceCfdTradingApiServiceTests;

[TestFixture]
public abstract class BinanceTradingServiceTestsFixture
{
    protected readonly ApiCredentials BinanceApiCredentials = Credentials.BinanceApiTestAccountApiCreds;
    protected readonly CurrencyPair CurrencyPair = new CurrencyPair("ETH", "BUSD");

    protected ICfdTradingApiService SUT = default!;
    protected decimal testMargin = 5;


    
    [OneTimeSetUp]
    public virtual void OneTimeSetUp() => this.SUT = new BinanceCfdTradingApiService(this.CurrencyPair, this.BinanceApiCredentials);

    [OneTimeTearDown]
    public virtual void OneTimeTearDown() { }
}
