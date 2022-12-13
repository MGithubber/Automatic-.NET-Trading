using System.Data.SqlClient;
using System.Globalization;

using AutomaticDotNETtrading.Application.Interfaces;
using AutomaticDotNETtrading.Application.Interfaces.Data;
using AutomaticDotNETtrading.Application.Interfaces.Services;
using AutomaticDotNETtrading.Application.Models;
using AutomaticDotNETtrading.Domain.Models;
using AutomaticDotNETtrading.Infrastructure.Data;
using AutomaticDotNETtrading.Infrastructure.Services;
using AutomaticDotNETtrading.Infrastructure.TradingStrategies.LuxAlgoAndPSAR.Implementations;
using AutomaticDotNETtrading.Infrastructure.TradingStrategies.LuxAlgoAndPSAR.Models;

using Binance.Net.Enums;

using CryptoExchange.Net.Authentication;

namespace Presentation.Api;

public static class Extensions
{
    private static LuxAlgoCandlestick DataWindowText_to_LuxAlgoCandlestick(string text)
    {
        List<string> data_window_lines = text.Replace("\r\n", "\n").Split('\n').ToList();
        List<string> desired_strings = new List<string>() { "Date", "Time", "Open", "Close", "High", "Low", "Buy", "Strong Buy", "Sell", "Strong Sell", "Exit Buy", "Exit Sell" };

        data_window_lines.RemoveAll(line => !desired_strings.Any(desired => line.StartsWith(desired)));
        desired_strings.ToList().ForEach(desired_str =>
        {
            int index = data_window_lines.FindIndex(item => item.StartsWith(desired_str)); // find index of desired string in list
            data_window_lines[index] = data_window_lines[index].Replace(desired_str, string.Empty);
        });

        return new LuxAlgoCandlestick
        {
            CurrencyPair = new CurrencyPair("ETH", "BUSD"),

            Date = DateTime.Parse(data_window_lines[1], CultureInfo.InvariantCulture),

            Open = decimal.Parse(data_window_lines[2], CultureInfo.InvariantCulture),
            High = decimal.Parse(data_window_lines[3], CultureInfo.InvariantCulture),
            Low = decimal.Parse(data_window_lines[4], CultureInfo.InvariantCulture),
            Close = decimal.Parse(data_window_lines[5], CultureInfo.InvariantCulture),

            Buy = decimal.Parse(data_window_lines[6], CultureInfo.InvariantCulture) == decimal.One,
            StrongBuy = decimal.Parse(data_window_lines[7], CultureInfo.InvariantCulture) == decimal.One,
            Sell = decimal.Parse(data_window_lines[8], CultureInfo.InvariantCulture) == decimal.One,
            StrongSell = decimal.Parse(data_window_lines[9], CultureInfo.InvariantCulture) == decimal.One,
            ExitBuy = double.Parse(data_window_lines[10].Replace("∅", "NaN").Replace("n/a", "NaN").Replace("N/A", "NaN"), CultureInfo.InvariantCulture),
            ExitSell = double.Parse(data_window_lines[11].Replace("∅", "NaN").Replace("n/a", "NaN").Replace("N/A", "NaN"), CultureInfo.InvariantCulture)
        };
    }

    public static void AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IDatabaseConnectionFactory<SqlConnection>, SqlDatabaseConnectionFactory>(_ => new SqlDatabaseConnectionFactory(configuration.GetConnectionString("BinanceTradingLogsDatabase")!));
        services.AddSingleton<ITradingDataDbService<LuxAlgoCandlestick>, TradingDataDbService>();
        
        services.AddSingleton<IChartDataService<LuxAlgoCandlestick>, TradingviewService<LuxAlgoCandlestick>>(_ =>
            TradingviewService<LuxAlgoCandlestick>.CreateAsync(
                new CurrencyPair("ETH", "BUSD"),
                KlineInterval.FifteenMinutes,
                DataWindowText_to_LuxAlgoCandlestick,
                new LuxAlgoCandlestickMap(),
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
