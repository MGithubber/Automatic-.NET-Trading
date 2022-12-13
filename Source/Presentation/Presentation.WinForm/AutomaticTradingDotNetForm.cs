using System;
using System.Data.SqlClient;
using System.Text;
using System.Xml.Serialization;

using AutomaticDotNETtrading.Application.Interfaces;
using AutomaticDotNETtrading.Application.Interfaces.Data;
using AutomaticDotNETtrading.Application.Interfaces.Services;
using AutomaticDotNETtrading.Application.Models;
using AutomaticDotNETtrading.Domain.Models;
using AutomaticDotNETtrading.Infrastructure.Data;
using AutomaticDotNETtrading.Infrastructure.Services;
using AutomaticDotNETtrading.Infrastructure.TradingStrategies.LuxAlgoAndPSAR;
using AutomaticDotNETtrading.Infrastructure.TradingStrategies.LuxAlgoAndPSAR.Implementations;
using AutomaticDotNETtrading.Infrastructure.TradingStrategies.LuxAlgoAndPSAR.Models;
using Binance.Net.Enums;
using Binance.Net.Objects.Models.Futures;

using CryptoExchange.Net.Objects;

using Microsoft.Extensions.DependencyInjection;

using Skender.Stock.Indicators;

namespace Presentation.WinForm;

public partial class AutomaticTradingDotNetForm : Form
{
    public AutomaticTradingDotNetForm() => this.InitializeComponent();

    private void AutomaticTradingDotNetForm_Load(object sender, EventArgs e) { }


    //// //// //// //// //// //// //// ////
    
    private IPoolTradingService? MPoolTradingService;
    private Task? PoolTradingTask;
    private Task? ShowActiveBotsTask;

    
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
    private void AddEvents(LuxAlgoAndPsarTradingStrategy LuxAlgoPsarStrategy)
    {
        LuxAlgoPsarStrategy.OnPositionOpened += LuxAlgoPsar_OnPositionOpened;
        LuxAlgoPsarStrategy.OnStopLossUpdated += LuxAlgoPsar_OnStopLossUpdated;
        LuxAlgoPsarStrategy.OnStopOutDetected += LuxAlgoPsar_OnStopOutDetected;
        LuxAlgoPsarStrategy.OnPositionClosed += LuxAlgoPsar_OnPositionClosed;
        
        void LuxAlgoPsar_OnPositionOpened(object? sender, KeyValuePair<LuxAlgoCandlestick, FuturesPosition> e)
        {
            Task.Run(() =>
            {
                _ = sender ?? throw new NullReferenceException($"{nameof(sender)} was NULL");

                BinanceFuturesOrder EntryOrder = e.Value.EntryOrder;
                BinanceFuturesOrder StopLossOrder = e.Value.StopLossOrder!;
                string SymbolName = EntryOrder.Symbol;
                string Side = EntryOrder.Side.ToString().ToUpper();

                StringBuilder builder = new StringBuilder();
                builder.AppendLine($"====  ====  ====  ====");
                builder.AppendLine($"{sender.GetType().Name}: POSITION OPENED on candlestick with date {e.Key.Date:dd/MM/yyyy HH:mm}");
                builder.AppendLine($"{Side} order placed on {SymbolName}");
                builder.AppendLine($"entry price == ${EntryOrder.AvgPrice}");
                builder.AppendLine($"quantity == {EntryOrder.Quantity}");
                builder.AppendLine($"stop loss price == ${StopLossOrder.StopPrice}");
                builder.AppendLine($"====  ====  ====  ====");
                this.OutputTextBox.BeginInvoke(() => this.OutputTextBox.Text = builder.ToString().ReplaceLineEndings());
            });
        }
        void LuxAlgoPsar_OnStopLossUpdated(object? sender, KeyValuePair<LuxAlgoCandlestick, BinanceFuturesPlacedOrder> e)
        {
            Task.Run(() =>
            {
                _ = sender ?? throw new NullReferenceException($"{nameof(sender)} was NULL");

                BinanceFuturesPlacedOrder StopLossOrder = e.Value;
                string SymbolName = StopLossOrder.Symbol;
                string Side = StopLossOrder.Side.ToString().ToUpper();

                StringBuilder builder = new StringBuilder();
                builder.AppendLine($"{sender.GetType().Name}: The stop loss has been updated on candlestick with date {e.Key.Date:dd/MM/yyyy HH:mm}");
                builder.AppendLine($"{Side} order placed on {SymbolName} on candlestick with date {e.Key.Date:dd/MM/yyyy HH:mm}");
                builder.AppendLine($"stop loss price == ${StopLossOrder.StopPrice}");
                this.OutputTextBox.BeginInvoke(() => this.OutputTextBox.Text = builder.ToString().ReplaceLineEndings());
            });
        }
        void LuxAlgoPsar_OnStopOutDetected(object? sender, LuxAlgoCandlestick e)
        {
            Task.Run(() =>
            {
                _ = sender ?? throw new NullReferenceException($"{nameof(sender)} was NULL");

                StringBuilder builder = new StringBuilder();
                builder.AppendLine($"{sender.GetType().Name}: STOP-OUT detected on candlestick with date {e.Date:dd/MM/yyyy HH:mm}");
                this.OutputTextBox.BeginInvoke(() => this.OutputTextBox.Text = builder.ToString().ReplaceLineEndings());
            });
        }
        void LuxAlgoPsar_OnPositionClosed(object? sender, KeyValuePair<LuxAlgoCandlestick, BinanceFuturesOrder> e)
        {
            Task.Run(() =>
            {
                _ = sender ?? throw new NullReferenceException($"{nameof(sender)} was NULL");

                BinanceFuturesOrder CloseOrder = e.Value;
                string SymbolName = CloseOrder.Symbol;
                string Side = CloseOrder.Side.ToString().ToUpper();

                StringBuilder builder = new StringBuilder();
                builder.AppendLine($"====  ====  ====  ====");
                builder.AppendLine($"{sender.GetType().Name}: POSITION CLOSED on candlestick with date {e.Key.Date:dd/MM/yyyy HH:mm}");
                builder.AppendLine($"{Side} order placed on {SymbolName} on candlestick with date {e.Key.Date:dd/MM/yyyy HH:mm}");
                builder.AppendLine($"quantity == ${CloseOrder.Quantity}");
                builder.AppendLine($"====  ====  ====  ====");
                this.OutputTextBox.BeginInvoke(() => this.OutputTextBox.Text = builder.ToString().ReplaceLineEndings());
            });
        }
    }
    private async Task StartPoolTradingAsync()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IDatabaseConnectionFactory<SqlConnection>, SqlDatabaseConnectionFactory>(_ => new SqlDatabaseConnectionFactory(ProgramIO.ConnectionString));
        services.AddSingleton<ITradingDataDbService<LuxAlgoCandlestick>, TradingDataDbService>();
        
        services.AddSingleton<IChartDataService<LuxAlgoCandlestick>, TradingviewChartDataService>(_ => 
            TradingviewChartDataService.CreateAsync(ProgramIO.UserDataDirectory.FullName,
                                                    ProgramIO.ChromeDownloadsDirectory.FullName)
                                                    .GetAwaiter().GetResult());
        
        services.AddSingleton<ITradingStrategy<LuxAlgoCandlestick>, LuxAlgoAndPsarTradingStrategyLong>(_ =>
        {
            TradingParameters tradingParameters = ReadXMLfile(ProgramIO.TradingParametersXmlFile_Long);
            BinanceCfdTradingApiService BinanceContractTrader = new(new CurrencyPair("ETH", "BUSD"), ProgramIO.BinanceApiCredentials);

            var LuxAlgoPsarStrategy = new LuxAlgoAndPsarTradingStrategyLong(tradingParameters, BinanceContractTrader);
            this.AddEvents(LuxAlgoPsarStrategy);

            return LuxAlgoPsarStrategy;
        });
        services.AddSingleton<ITradingStrategy<LuxAlgoCandlestick>, LuxAlgoAndPsarTradingStrategyShort>(_ =>
        {
            TradingParameters tradingParameters = ReadXMLfile(ProgramIO.TradingParametersXMLFile_Short);
            BinanceCfdTradingApiService BinanceContractTrader = new(new CurrencyPair("ETH", "USDT"), ProgramIO.BinanceApiCredentials);

            var LuxAlgoPsarStrategy = new LuxAlgoAndPsarTradingStrategyShort(tradingParameters, BinanceContractTrader);
            this.AddEvents(LuxAlgoPsarStrategy);

            return LuxAlgoPsarStrategy;
        });

        services.AddSingleton<IPoolTradingService, MPoolTradingService<LuxAlgoCandlestick, SqlConnection>>();


        this.MPoolTradingService = services.BuildServiceProvider().GetRequiredService<IPoolTradingService>();
        await this.MPoolTradingService!.StartTradingAsync();
    }
    
    private void StartButton_Click(object sender, EventArgs e)
    {
        this.PoolTradingTask ??= this.StartPoolTradingAsync();
        this.ShowActiveBotsTask ??= Task.Run(() =>
        {
            while (this.MPoolTradingService is null) continue;
            this.NrActiveBotsTextBox.BeginInvoke(() => this.NrActiveBotsTextBox.Text = this.MPoolTradingService.NrTradingStrategies.ToString());
        }); 
    }
    

    //// //// //// //// ////


    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        this.MPoolTradingService?.Dispose();
        base.OnFormClosed(e);
    }
}
