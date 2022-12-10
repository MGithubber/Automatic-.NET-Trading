using AutomaticDotNETtrading.Application.Interfaces.Data;
using AutomaticDotNETtrading.Application.Interfaces.Services;
using AutomaticDotNETtrading.Application.Interfaces;
using AutomaticDotNETtrading.Application.Models;
using AutomaticDotNETtrading.Domain.Models;
using AutomaticDotNETtrading.Infrastructure.Data;
using AutomaticDotNETtrading.Infrastructure.Services;
using AutomaticDotNETtrading.Infrastructure.TradingStrategies.LuxAlgoAndPSAR.Implementations;
using AutomaticDotNETtrading.Infrastructure.TradingStrategies.LuxAlgoAndPSAR.Models;
using System.Data.SqlClient;
using System.Xml.Serialization;

namespace Presentation.Api;

public static class Extensions
{
    private static TradingParameters ReadXMLfile(FileInfo xmlFile)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(TradingParameters));
        using FileStream stream = xmlFile.OpenRead();
        return (TradingParameters)serializer.Deserialize(stream)!;
    }
    public static void AddServices(this IServiceCollection services, IConfiguration configuration)
    {
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
    }

    public static void MapEndpoints(this IEndpointRouteBuilder app)
    {
        //app.MapGet("/weatherforecast", () => { })
        //.WithName("GetWeatherForecast")
        //.WithOpenApi();
    }
}
