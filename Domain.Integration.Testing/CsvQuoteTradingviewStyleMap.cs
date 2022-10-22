using System.Globalization;
using System.Reflection;

using CsvHelper.Configuration;

using CsvHelper;
using Skender.Stock.Indicators;

namespace Domain.Integration.Testing;
 
/// <summary>
/// <para>Represents a map matching the format in which https://www.tradingview.com exports the chart data containing information about objects of type <see cref="TVCandlestick"/></para>
/// <para>Can be used for both reading and writing and uses <see cref="CultureInfo.InvariantCulture"/> by default</para>
/// </summary>
public class CsvQuoteTradingviewStyleMap : ClassMap<Quote>
{
    private static DateTime ParseDateTime(ConvertFromStringArgs args)
    {
        string string_to_parse = args.Row.GetField("time")!;

        if (long.TryParse(string_to_parse, out long seconds_from_epoch))
            return new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(seconds_from_epoch);

        if (DateTime.TryParse(string_to_parse, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsed))
            return parsed;

        throw new ReaderException(args.Row.Context, $"The specified string \"{string_to_parse}\" was not recognized as a valid {nameof(CultureInfo.InvariantCulture)} {nameof(DateTime)} string representation", new FormatException("The string was not recognized as a valid DateTime"));
    }
    
    public CsvQuoteTradingviewStyleMap()
    {
        this.Map(candlestick => candlestick.Date).Name("time").Convert(args => ParseDateTime(args));

        this.Map(candlestick => candlestick.Open).Name("open").Convert(args => decimal.Parse(args.Row.GetField("open")!, CultureInfo.InvariantCulture));
        this.Map(candlestick => candlestick.Close).Name("close").Convert(args => decimal.Parse(args.Row.GetField("close")!, CultureInfo.InvariantCulture));
        this.Map(candlestick => candlestick.High).Name("high").Convert(args => decimal.Parse(args.Row.GetField("high")!, CultureInfo.InvariantCulture));
        this.Map(candlestick => candlestick.Low).Name("low").Convert(args => decimal.Parse(args.Row.GetField("low")!, CultureInfo.InvariantCulture));

        //// //// ////

        // this.Map(candlestick => candlestick.Date).Index(0).Name("time").Convert(args => args.Value.Date.ToString(CultureInfo.InvariantCulture));
        this.Map(candlestick => candlestick.Date).Index(0).Name("time").Convert(args => args.Value.Date.ToString(CultureInfo.InvariantCulture));

        this.Map(candlestick => candlestick.Open).Index(1).Name("open").Convert(args => args.Value.Open.ToString(CultureInfo.InvariantCulture));
        this.Map(candlestick => candlestick.Close).Index(2).Name("close").Convert(args => args.Value.Close.ToString(CultureInfo.InvariantCulture));
        this.Map(candlestick => candlestick.High).Index(3).Name("high").Convert(args => args.Value.High.ToString(CultureInfo.InvariantCulture));
        this.Map(candlestick => candlestick.Low).Index(4).Name("low").Convert(args => args.Value.Low.ToString(CultureInfo.InvariantCulture));
    }
}
