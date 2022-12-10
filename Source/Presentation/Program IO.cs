using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Binance.Net.Objects.Models.Futures;

using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.CommonObjects;

namespace Presentation.WinForm;

internal static class ProgramIO
{
    private const string ProcessFiles = @"..\..\Process files";

    public static readonly DirectoryInfo ChromeDriverDirectory = new DirectoryInfo($@"{ProcessFiles}\Chrome driver");
    public static readonly DirectoryInfo ChromeDownloadsDirectory = new DirectoryInfo($@"{ProcessFiles}\Chrome driver\Downloads");

    public static readonly DirectoryInfo UserDataDirectory = new DirectoryInfo($@"C:\Users\{Environment.UserName}\AppData\Local\Google\Chrome\User Data");

    public static readonly DirectoryInfo InstructionsDirectory = new DirectoryInfo($@"{ProcessFiles}\Instructions");
    public static readonly FileInfo StartInstructionsFile = new FileInfo($@"{ProcessFiles}\Instructions\program start.txt");

    public static readonly DirectoryInfo LogsDirectory = new DirectoryInfo($@"{ProcessFiles}\Logs");

    public static readonly DirectoryInfo SelectorsDirectory = new DirectoryInfo($@"{ProcessFiles}\Selectors");
    public static readonly DirectoryInfo XPathSelectorsDirectory = new DirectoryInfo($@"{ProcessFiles}\Selectors\XPath");

    public static readonly DirectoryInfo TradingParametersDirectory = new DirectoryInfo($@"{ProcessFiles}\Trading Parameters");
    public static readonly FileInfo TradingParametersXmlFile_Long = new FileInfo($@"{ProcessFiles}\Trading Parameters\LONG parameters.txt");
    public static readonly FileInfo TradingParametersXMLFile_Short = new FileInfo($@"{ProcessFiles}\Trading Parameters\SHORT parameters.txt");

    /////
    
    public static readonly ApiCredentials BinanceApiCredentials = new ApiCredentials(File.ReadAllText($@"{ProcessFiles}\ApiCredentials\public key.txt"), File.ReadAllText($@"{ProcessFiles}\ApiCredentials\private key.txt"));

    public const string ConnectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=""Binance trading logs"";Integrated Security=True";
}
