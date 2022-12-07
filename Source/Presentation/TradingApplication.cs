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

namespace Presentation;

internal class TradingApplication
{
    private static TradingParameters ReadXMLfile(FileInfo xmlFile)
    {
        TradingParameters parameters;
        using (FileStream stream = xmlFile.OpenRead())
        {
            XmlSerializer serializer = new XmlSerializer(typeof(TradingParameters));
            parameters = (TradingParameters)serializer.Deserialize(stream)!;
        }

        return parameters;
    }
    public static ServiceCollection GetDefaultServices()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IDatabaseConnectionFactory<SqlConnection>, SqlDatabaseConnectionFactory>(x => new(ProgramIO.ConnectionString));
        services.AddSingleton<ITradingDataDbService<LuxAlgoCandlestick>, TradingDataDbService>();

        services.AddSingleton<IChartDataService<LuxAlgoCandlestick>, TradingviewChartDataService>(x =>
            new TradingviewChartDataService(
                chromeDriverDirectory: ProgramIO.ChromeDriverDirectory.FullName,
                userDataDirectory: ProgramIO.UserDataDirectory.FullName,
                downloadsDirectory: ProgramIO.ChromeDownloadsDirectory.FullName,
                Chart_Locator: By.ClassName("chart-gui-wrapper"),
                DataWindow_Locator: By.ClassName("chart-data-window"),
                ZoomInButton_Locator: By.ClassName("control-bar__btn--zoom-in"),
                ZoomOutButton_Locator: By.ClassName("control-bar__btn--zoom-out"),
                ScrollLeftButton_Locator: By.ClassName("control-bar__btn--move-left"),
                ScrollRightButton_Locator: By.ClassName("control-bar__btn--move-right"),
                ResetChartButton_Locator: By.ClassName("control-bar__btn--turn-button"),
                ManageLayoutsButton_Locator: By.ClassName("js-save-load-menu-open-button"),
                ExportChartDataButton_Locator: By.XPath(File.ReadAllText(Path.Combine(ProgramIO.XPathSelectorsDirectory.FullName, "ExportChartDataButton_Locator.txt"))),
                ExportChartDataConfirmButton_Locator: By.XPath(File.ReadAllText(Path.Combine(ProgramIO.XPathSelectorsDirectory.FullName, "ExportChartDataConfirmButton_Locator.txt")))));

        services.AddSingleton<ITradingStrategy<LuxAlgoCandlestick>, LuxAlgoAndPsarTradingStrategyLong>(provider =>
        {
            TradingParameters TradingParams_LONG = ReadXMLfile(ProgramIO.TradingParametersXmlFile_Long);
            BinanceCfdTradingApiService BinanceContractTrader_Long = new(new CurrencyPair("ETH", "BUSD"), ProgramIO.BinanceApiCredentials);

            return new(TradingParams_LONG, BinanceContractTrader_Long);
        });
        services.AddSingleton<ITradingStrategy<LuxAlgoCandlestick>, LuxAlgoAndPsarTradingStrategyShort>(provider =>
        {
            TradingParameters TradingParams_SHORT = ReadXMLfile(ProgramIO.TradingParametersXMLFile_Short);
            BinanceCfdTradingApiService BinanceContractTrader_Short = new(new CurrencyPair("ETH", "USDT"), ProgramIO.BinanceApiCredentials);
            return new(TradingParams_SHORT, BinanceContractTrader_Short);
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
