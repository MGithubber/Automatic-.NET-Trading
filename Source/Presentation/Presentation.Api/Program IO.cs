using System.Net;

using CryptoExchange.Net.Authentication;

namespace Presentation.Api;

internal static class ProgramIO
{
    private const string ProcessFiles = @"bin\Process files";

    public static readonly DirectoryInfo ChromeDriverDirectory = new DirectoryInfo($@"{ProcessFiles}\Chrome driver");
    public static readonly DirectoryInfo ChromeDownloadsDirectory = new DirectoryInfo($@"{ProcessFiles}\Chrome driver\Downloads");

    public static readonly DirectoryInfo UserDataDirectory = new DirectoryInfo($@"C:\Users\{Environment.UserName}\AppData\Local\Google\Chrome\User Data");
}
