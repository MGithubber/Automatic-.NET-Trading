using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AutomaticDotNETtrading.Application.Interfaces.Data;
using AutomaticDotNETtrading.Application.Interfaces.Services;
using AutomaticDotNETtrading.Application.Interfaces;
using AutomaticDotNETtrading.Application.Models;

using AutomaticDotNETtrading.Infrastructure.TradingStrategies.LuxAlgoAndPSAR.Models;

using Microsoft.Extensions.DependencyInjection;
using AutomaticDotNETtrading.Infrastructure.Data;
using AutomaticDotNETtrading.Infrastructure.Services;
using AutomaticDotNETtrading.Infrastructure.TradingStrategies.LuxAlgoAndPSAR.Implementations;
using OpenQA.Selenium;
using AutomaticDotNETtrading.Domain.Models;
using System.Xml.Serialization;
using Binance.Net.Objects.Models.Futures;

namespace Presentation.WinForm;

internal class TradingApplication
{
    private static TradingParameters ReadXMLfile(FileInfo xmlFile)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(TradingParameters));
        using FileStream stream = xmlFile.OpenRead();
        return (TradingParameters)serializer.Deserialize(stream)!;
    }
    public static ServiceCollection GetDefaultServices()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IDatabaseConnectionFactory<SqlConnection>, SqlDatabaseConnectionFactory>(_ => new SqlDatabaseConnectionFactory(ProgramIO.ConnectionString));
        services.AddSingleton<ITradingDataDbService<LuxAlgoCandlestick>, TradingDataDbService>();
        
        services.AddSingleton<IChartDataService<LuxAlgoCandlestick>, TradingviewChartDataService>(_ =>
        {
            return new TradingviewChartDataService(
                chromeDriverDirectory: ProgramIO.ChromeDriverDirectory.FullName,
                userDataDirectory: ProgramIO.UserDataDirectory.FullName,
                downloadsDirectory: ProgramIO.ChromeDownloadsDirectory.FullName);
        });

        services.AddSingleton<ITradingStrategy<LuxAlgoCandlestick>, LuxAlgoAndPsarTradingStrategyLong>(_ =>
        {
            TradingParameters tradingParameters = ReadXMLfile(ProgramIO.TradingParametersXmlFile_Long);
            BinanceCfdTradingApiService BinanceContractTrader = new(new CurrencyPair("ETH", "BUSD"), ProgramIO.BinanceApiCredentials);

            var LuxAlgoPsarStrategy = new LuxAlgoAndPsarTradingStrategyLong(tradingParameters, BinanceContractTrader);
            // // to add events // //

            return LuxAlgoPsarStrategy;
        });
        services.AddSingleton<ITradingStrategy<LuxAlgoCandlestick>, LuxAlgoAndPsarTradingStrategyShort>(_ =>
        {
            TradingParameters tradingParameters = ReadXMLfile(ProgramIO.TradingParametersXMLFile_Short);
            BinanceCfdTradingApiService BinanceContractTrader = new(new CurrencyPair("ETH", "USDT"), ProgramIO.BinanceApiCredentials);

            var LuxAlgoPsarStrategy = new LuxAlgoAndPsarTradingStrategyShort(tradingParameters, BinanceContractTrader);
            // // to add events // //

            return LuxAlgoPsarStrategy;
        });

        services.AddSingleton<IPoolTradingService, MPoolTradingService<LuxAlgoCandlestick, SqlConnection>>();

        return services;
    }

    public static IPoolTradingService GetDefaultPoolTradingService()
    {
        var services = GetDefaultServices();

        var serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<IPoolTradingService>();
    }
}
