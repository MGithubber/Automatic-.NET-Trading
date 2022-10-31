using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Binance.Net.Objects.Models.Futures;

using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.CommonObjects;

namespace Presentation;

internal static class ProgramIO
{
    public static readonly DirectoryInfo ChromeDriverDirectory = new DirectoryInfo(@"Process files\Chrome driver");
    public static readonly DirectoryInfo ChromeDownloadsDirectory = new DirectoryInfo(@"Process files\Chrome driver\Downloads");

    public static readonly DirectoryInfo UserDataDirectory = new DirectoryInfo($@"C:\Users\{Environment.UserName}\AppData\Local\Google\Chrome\User Data");

    public static readonly DirectoryInfo InstructionsDirectory = new DirectoryInfo(@"Process files\Instructions");
    public static readonly FileInfo StartInstructionsFile = new FileInfo(@"Process files\Instructions\program start.txt");

    public static readonly DirectoryInfo LogsDirectory = new DirectoryInfo(@"Process files\Logs");

    public static readonly DirectoryInfo SelectorsDirectory = new DirectoryInfo(@"Process files\Selectors");
    public static readonly DirectoryInfo XPathSelectorsDirectory = new DirectoryInfo(@"Process files\Selectors\XPath");

    public static readonly DirectoryInfo TradingParametersDirectory = new DirectoryInfo(@"Process files\Trading Parameters");
    public static readonly FileInfo TradingParametersXmlFile_Long = new FileInfo(@"Process files\Trading Parameters\LONG parameters.txt");
    public static readonly FileInfo TradingParametersXMLFile_Short = new FileInfo(@"Process files\Trading Parameters\SHORT parameters.txt");
    
    /////
    
    public static readonly ApiCredentials BinanceApiCredentials = new ApiCredentials(File.ReadAllText(@"Process files\ApiCredentials\public key.txt"), File.ReadAllText(@"Process files\ApiCredentials\private key.txt"));
    
    public const string ConnectionString = @"Data Source=(localdb)\MSSQLLocalDB;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Connect Timeout=60;Encrypt=False;TrustServerCertificate=False";
    public const string DatabaseName = "Binance trading logs";
}
