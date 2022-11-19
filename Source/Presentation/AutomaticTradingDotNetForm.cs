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

using OpenQA.Selenium;

using System;
using System.Data.SqlClient;
using System.Text;
using System.Xml.Serialization;

namespace Presentation;

public partial class AutomaticTradingDotNetForm : Form
{
    public AutomaticTradingDotNetForm() => this.InitializeComponent();

    private void AutomaticTradingDotNetForm_Load(object sender, EventArgs e) { }


    //// //// //// //// //// //// //// ////

    private MPoolTradingService<LuxAlgoCandlestick, SqlConnection>? MPoolTradingService;
    private async Task StartPoolTradingAsync()
    {
        static TradingviewChartDataService Create_TradingviewDataExtractor()
        {
            return new TradingviewChartDataService(
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
                ExportChartDataConfirmButton_Locator: By.XPath(File.ReadAllText(Path.Combine(ProgramIO.XPathSelectorsDirectory.FullName, "ExportChartDataConfirmButton_Locator.txt"))));
        }
        static TradingParameters ReadXMLfile(FileInfo xmlFile)
        {
            TradingParameters parameters;
            using (FileStream stream = xmlFile.OpenRead())
            {
                XmlSerializer serializer = new XmlSerializer(typeof(TradingParameters));
                parameters = (TradingParameters)serializer.Deserialize(stream)!;
            }

            return parameters;
        }
        
        //// //// //// //// ////
        
        TradingviewChartDataService TradingviewChartDataService = Create_TradingviewDataExtractor();

        //// ////

        #region LuxAlgo and Psar divergence
        List<LuxAlgoAndPsarTradingStrategy> LuxAlgoPsarMethodsList = new List<LuxAlgoAndPsarTradingStrategy>();


        TradingParameters TradingParams_LONG = ReadXMLfile(ProgramIO.TradingParametersXmlFile_Long);
        BinanceCfdTradingApiService BinanceContractTrader_Long = new(new CurrencyPair("ETH", "BUSD"), ProgramIO.BinanceApiCredentials);
        LuxAlgoPsarMethodsList.Add(new LuxAlgoAndPsarTradingStrategyLong(TradingParams_LONG, BinanceContractTrader_Long));
        
        TradingParameters TradingParams_SHORT = ReadXMLfile(ProgramIO.TradingParametersXMLFile_Short);
        BinanceCfdTradingApiService BinanceContractTrader_Short = new(new CurrencyPair("ETH", "USDT"), ProgramIO.BinanceApiCredentials);
        LuxAlgoPsarMethodsList.Add(new LuxAlgoAndPsarTradingStrategyShort(TradingParams_SHORT, BinanceContractTrader_Short));

        
        LuxAlgoPsarMethodsList.ForEach(LuxAlgoPsar =>
        {
            LuxAlgoPsar.OnPositionOpened += LuxAlgoPsar_OnPositionOpened;
            LuxAlgoPsar.OnStopLossUpdated += LuxAlgoPsar_OnStopLossUpdated;
            LuxAlgoPsar.OnStopOutDetected += LuxAlgoPsar_OnStopOutDetected;
            LuxAlgoPsar.OnPositionClosed += LuxAlgoPsar_OnPositionClosed;
        });
        #region LuxAlgoAndPsarTradingStrategy events
        void LuxAlgoPsar_OnPositionOpened(object? sender, KeyValuePair<LuxAlgoCandlestick, FuturesPosition> e)
        {
            Task.Run(() =>
            {
                _ = sender ?? throw new NullReferenceException($"{nameof(sender)} was NULL");

                BinanceFuturesOrder EntryOrder = e.Value.EntryOrder;
                BinanceFuturesOrder? StopLossOrder = e.Value.StopLossOrder;
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
        #endregion
        #endregion

        //// ////
        
        SqlDatabaseConnectionFactory SqlDatabaseConnectionFactory = new SqlDatabaseConnectionFactory(ProgramIO.ConnectionString);
        TradingDataDbService TradingDataDbService = new TradingDataDbService(SqlDatabaseConnectionFactory);
        
        //// ////

        this.MPoolTradingService = new(TradingviewChartDataService, TradingDataDbService, LuxAlgoPsarMethodsList.ToArray());
        this.MPoolTradingService.OnNewCandlestickRegistered += new EventHandler<LuxAlgoCandlestick>((sender, e) =>
        {
            Task.Run(() =>
            {
                _ = sender ?? throw new NullReferenceException($"{nameof(sender)} was NULL");
                string newcandlestring = $"{sender.GetType().Name} registered candlestick at date {e.Date:dd/MM/yyyy HH:mm}, {nameof(e.LuxAlgoSignal)}=={e.LuxAlgoSignal}\n";
                this.OutputTextBox.BeginInvoke(() => this.OutputTextBox.Text += newcandlestring.ReplaceLineEndings());
            });
        });
        
        try { await this.MPoolTradingService.StartTradingAsync(); }
        catch (Exception exception) { _ = Task.Run(() => MessageBox.Show(exception.ToString(), exception.GetType().Name, MessageBoxButtons.OK, MessageBoxIcon.Error)); }
        finally { this.MPoolTradingService.QuitChartDataService(); }
    }
    
    private Task? PoolTradingTask = null;
    private Task? ShowActiveBotsTask = null;
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
        this.MPoolTradingService?.QuitChartDataService();
        base.OnFormClosed(e);
    }
}
