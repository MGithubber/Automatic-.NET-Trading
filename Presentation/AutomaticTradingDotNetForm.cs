using AutomaticDotNETtrading.Application.Models;
using AutomaticDotNETtrading.Domain.Models;
using AutomaticDotNETtrading.Infrastructure.Data;
using AutomaticDotNETtrading.Infrastructure.Models;
using AutomaticDotNETtrading.Infrastructure.Services;
using AutomaticDotNETtrading.Infrastructure.TradingStrategies.LuxAlgoAndPSAR;

using Binance.Net.Enums;
using Binance.Net.Objects.Models.Futures;

using CryptoExchange.Net.Objects;

using OpenQA.Selenium;

using System.Data.SqlClient;
using System.Text;
using System.Xml.Serialization;

namespace Presentation;

public partial class AutomaticTradingDotNetForm : Form
{
    public AutomaticTradingDotNetForm() => InitializeComponent();

    private void AutomaticTradingDotNetForm_Load(object sender, EventArgs e) { }
    
    
    //// //// //// //// //// //// //// ////
    

    private readonly object EventsPadLock = new object();
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
        void LuxAlgoPsar_OnPositionOpened(object? sender, KeyValuePair<TVCandlestick, IEnumerable<BinanceFuturesPlacedOrder>> e)
        {
            lock (this.EventsPadLock)
            {
                Task.Run(() =>
                {
                    _ = sender ?? throw new NullReferenceException($"{nameof(sender)} was NULL");

                    BinanceFuturesPlacedOrder EntryOrder = e.Value.First();
                    BinanceFuturesPlacedOrder StopLossOrder = e.Value.ElementAt(1);
                    string SymbolName = EntryOrder.Symbol;
                    string Side = EntryOrder.Side.ToString().ToUpper();

                    StringBuilder builder = new StringBuilder();
                    builder.AppendLine($"====  ====  ====  ====");
                    builder.AppendLine($"{sender.GetType().Name}: POSITION OPENED on candlestick with date {e.Key.Date:dd/MM/yyyy HH:mm}");
                    builder.AppendLine($"{Side} order placed on {SymbolName}");
                    builder.AppendLine($"entry price == ${EntryOrder.AveragePrice}");
                    builder.AppendLine($"quantity == {EntryOrder.Quantity}");
                    builder.AppendLine($"stop loss price == ${StopLossOrder.StopPrice}");
                    builder.AppendLine($"====  ====  ====  ====");
                    this.OutputTextBox.Text = builder.ToString().ReplaceLineEndings();
                });
            }
        }
        void LuxAlgoPsar_OnStopLossUpdated(object? sender, KeyValuePair<TVCandlestick, BinanceFuturesPlacedOrder> e)
        {
            lock (this.EventsPadLock)
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
                    this.OutputTextBox.Text = builder.ToString().ReplaceLineEndings();
                });
            }
        }
        void LuxAlgoPsar_OnStopOutDetected(object? sender, TVCandlestick e)
        {
            lock (this.EventsPadLock)
            {
                Task.Run(() =>
                {
                    _ = sender ?? throw new NullReferenceException($"{nameof(sender)} was NULL");

                    StringBuilder builder = new StringBuilder();
                    builder.AppendLine($"{sender.GetType().Name}: STOP-OUT detected on candlestick with date {e.Date:dd/MM/yyyy HH:mm}");
                    this.OutputTextBox.Text = builder.ToString().ReplaceLineEndings();
                });
            }
        }
        void LuxAlgoPsar_OnPositionClosed(object? sender, KeyValuePair<TVCandlestick, BinanceFuturesPlacedOrder> e)
        {
            lock (this.EventsPadLock)
            {
                Task.Run(() =>
                {
                    _ = sender ?? throw new NullReferenceException($"{nameof(sender)} was NULL");

                    BinanceFuturesPlacedOrder CloseOrder = e.Value;
                    string SymbolName = CloseOrder.Symbol;
                    string Side = CloseOrder.Side.ToString().ToUpper();

                    StringBuilder builder = new StringBuilder();
                    builder.AppendLine($"====  ====  ====  ====");
                    builder.AppendLine($"{sender.GetType().Name}: POSITION CLOSED on candlestick with date {e.Key.Date:dd/MM/yyyy HH:mm}");
                    builder.AppendLine($"{Side} order placed on {SymbolName} on candlestick with date {e.Key.Date:dd/MM/yyyy HH:mm}");
                    builder.AppendLine($"quantity == ${CloseOrder.Quantity}");
                    builder.AppendLine($"====  ====  ====  ====");
                    this.OutputTextBox.Text = builder.ToString().ReplaceLineEndings();
                });
            }
        }
        #endregion
        #endregion

        //// ////

        SqlDatabaseConnectionFactory SqlDatabaseConnectionFactory = new SqlDatabaseConnectionFactory(String.Empty, String.Empty);
        TradingDataDbService TradingDataDbService = new TradingDataDbService(SqlDatabaseConnectionFactory);
        
        //// ////

        MPoolTradingService<TVCandlestick, SqlConnection> MPoolTradingService = new(TradingviewChartDataService, TradingDataDbService, LuxAlgoPsarMethodsList.ToArray());
        MPoolTradingService.OnNewCandlestickRegistered += new EventHandler<TVCandlestick>((sender, e) =>
        {
            if (sender is null)
                throw new NullReferenceException($"An event was invoked but the {nameof(sender)} was NULL");

            Task.Run(() =>
            {
                string newcandlestring = $"{sender.GetType().Name} registered candlestick at date {e.Date:dd/MM/yyyy HH:mm}, {nameof(e.LuxAlgoSignal)}=={e.LuxAlgoSignal}";
            });
        });
        
        try { await MPoolTradingService.StartTradingAsync(); }
        catch { throw; }
        finally { MPoolTradingService.QuitChartDataService(); }
    }

    private Task? PoolTradingTask = null;
    private void StartButton_Click(object sender, EventArgs e)
    {
        this.PoolTradingTask ??= this.StartPoolTradingAsync();
    }


    //// //// //// //// ////


    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        base.OnFormClosed(e);
    }
}
