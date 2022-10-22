using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

// // // CSV HELPER .NET // // //
using CsvHelper;
using CsvHelper.Configuration;

namespace AutomaticDotNETtrading.Infrastructure.Models;

/// <summary>
/// <para>Represents a map matching the format in which https://www.tradingview.com exports the chart data containing information about objects of type <see cref="TVCandlestick"/></para>
/// <para>Can be used for both reading and writing and uses <see cref="CultureInfo.InvariantCulture"/> by default</para>
/// </summary>
public class CsvTVCandlestickTradingviewStyleMap : ClassMap<TVCandlestick>
{
    private static DateTime ParseDateTime(ConvertFromStringArgs args)
    {
        string string_to_parse = args.Row.GetField("time");

        if (long.TryParse(string_to_parse, out long seconds_from_epoch))
            return new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(seconds_from_epoch);

        if (DateTime.TryParse(string_to_parse, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsed))
            return parsed;

        throw new ReaderException(args.Row.Context, $"The specified string \"{string_to_parse}\" was not recognized as a valid {nameof(CultureInfo.InvariantCulture)} {nameof(DateTime)} string representation", new FormatException("The string was not recognized as a valid DateTime"));
    }

    public CsvTVCandlestickTradingviewStyleMap()
    {
        this.Map(candlestick => candlestick.Date).Name("time").Convert(args => ParseDateTime(args));

        this.Map(candlestick => candlestick.Open).Name("open").Convert(args => decimal.Parse(args.Row.GetField("open"), CultureInfo.InvariantCulture));
        this.Map(candlestick => candlestick.Close).Name("close").Convert(args => decimal.Parse(args.Row.GetField("close"), CultureInfo.InvariantCulture));
        this.Map(candlestick => candlestick.High).Name("high").Convert(args => decimal.Parse(args.Row.GetField("high"), CultureInfo.InvariantCulture));
        this.Map(candlestick => candlestick.Low).Name("low").Convert(args => decimal.Parse(args.Row.GetField("low"), CultureInfo.InvariantCulture));

        this.Map(candlestick => candlestick.Buy).Name("Buy");
        this.Map(candlestick => candlestick.StrongBuy).Name("Strong Buy");
        this.Map(candlestick => candlestick.Sell).Name("Sell");
        this.Map(candlestick => candlestick.StrongSell).Name("Strong Sell");
        this.Map(candlestick => candlestick.ExitBuy).Name("Exit Buy").Convert(args => double.Parse(args.Row.GetField("Exit Buy").Replace("∅", "NaN").Replace("n/a", "NaN").Replace("N/A", "NaN"), CultureInfo.InvariantCulture));
        this.Map(candlestick => candlestick.ExitSell).Name("Exit Sell").Convert(args => double.Parse(args.Row.GetField("Exit Sell").Replace("∅", "NaN").Replace("n/a", "NaN").Replace("N/A", "NaN"), CultureInfo.InvariantCulture));
        
        //// //// ////

        // this.Map(candlestick => candlestick.Date).Index(0).Name("time").Convert(args => args.Value.Date.ToString(CultureInfo.InvariantCulture));
        this.Map(candlestick => candlestick.Date).Index(0).Name("time").Convert(args => args.Value.Date.ToString(CultureInfo.InvariantCulture));

        this.Map(candlestick => candlestick.Open).Index(1).Name("open").Convert(args => args.Value.Open.ToString(CultureInfo.InvariantCulture));
        this.Map(candlestick => candlestick.Close).Index(2).Name("close").Convert(args => args.Value.Close.ToString(CultureInfo.InvariantCulture));
        this.Map(candlestick => candlestick.High).Index(3).Name("high").Convert(args => args.Value.High.ToString(CultureInfo.InvariantCulture));
        this.Map(candlestick => candlestick.Low).Index(4).Name("low").Convert(args => args.Value.Low.ToString(CultureInfo.InvariantCulture));

        //this.Map(candlestick => candlestick.Buy).Index(5).Name("Lux Algo signal").Convert(args => args.Value.LuxAlgoSignal.ToString());
        this.Map(candlestick => candlestick.Buy).Index(5).Name("Buy");
        this.Map(candlestick => candlestick.StrongBuy).Index(6).Name("Strong Buy");
        this.Map(candlestick => candlestick.Sell).Index(7).Name("Sell");
        this.Map(candlestick => candlestick.StrongSell).Index(8).Name("Strong Sell");
        this.Map(candlestick => candlestick.ExitBuy).Index(9).Name("Exit Buy").Convert(args => args.Value.ExitBuy.ToString(CultureInfo.InvariantCulture));
        this.Map(candlestick => candlestick.ExitSell).Index(10).Name("Exit Sell").Convert(args => args.Value.ExitSell.ToString(CultureInfo.InvariantCulture));
    }
}
