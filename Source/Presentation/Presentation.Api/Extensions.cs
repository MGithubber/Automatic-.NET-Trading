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
using CryptoExchange.Net.Authentication;
using Microsoft.Extensions.Configuration;

namespace Presentation.Api;

public static class Extensions
{
    //private static TradingParameters ReadXMLfile(FileInfo xmlFile)
    //{
    //    XmlSerializer serializer = new XmlSerializer(typeof(TradingParameters));
    //    using FileStream stream = xmlFile.OpenRead();
    //    return (TradingParameters)serializer.Deserialize(stream)!;
    //}
    public static void AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IDatabaseConnectionFactory<SqlConnection>, SqlDatabaseConnectionFactory>(_ => new SqlDatabaseConnectionFactory(configuration.GetConnectionString("BinanceTradingLogsDatabase")!));
        services.AddSingleton<ITradingDataDbService<LuxAlgoCandlestick>, TradingDataDbService>();

        services.AddSingleton<IChartDataService<LuxAlgoCandlestick>, TradingviewChartDataService>(_ =>
            TradingviewChartDataService.CreateAsync(
                ProgramIO.UserDataDirectory.FullName,
                ProgramIO.ChromeDownloadsDirectory.FullName)
                .GetAwaiter().GetResult());

        services.AddSingleton<ITradingStrategy<LuxAlgoCandlestick>, LuxAlgoAndPsarTradingStrategyLong>(_ =>
        {
            TradingParameters tradingParameters = configuration.GetRequiredSection("LuxAlgoAndPsarTradingStrategyLong:TradingParameters").Get<TradingParameters>()!;
            BinanceCfdTradingApiService BinanceContractTrader = new BinanceCfdTradingApiService(
                configuration.GetRequiredSection("LuxAlgoAndPsarTradingStrategyLong:CurrencyPair").Get<CurrencyPair>()!,
                new ApiCredentials(configuration.GetValue<string>("BinanceApiCredentials:public")!, configuration.GetValue<string>("BinanceApiCredentials:private")!));

            var LuxAlgoPsarStrategy = new LuxAlgoAndPsarTradingStrategyLong(tradingParameters, BinanceContractTrader);
            // // to add events // //
            
            return LuxAlgoPsarStrategy;
        });
        services.AddSingleton<ITradingStrategy<LuxAlgoCandlestick>, LuxAlgoAndPsarTradingStrategyShort>(_ =>
        {
            TradingParameters tradingParameters = configuration.GetRequiredSection("LuxAlgoAndPsarTradingStrategyShort:TradingParameters").Get<TradingParameters>()!;
            BinanceCfdTradingApiService BinanceContractTrader = new BinanceCfdTradingApiService(
                configuration.GetRequiredSection("LuxAlgoAndPsarTradingStrategyShort:CurrencyPair").Get<CurrencyPair>()!,
                new ApiCredentials(configuration.GetValue<string>("BinanceApiCredentials:public")!, configuration.GetValue<string>("BinanceApiCredentials:private")!));

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
