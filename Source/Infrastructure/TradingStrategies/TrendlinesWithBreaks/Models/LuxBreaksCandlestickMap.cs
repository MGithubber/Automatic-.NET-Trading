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

namespace AutomaticDotNETtrading.Infrastructure.TradingStrategies.TrendlinesWithBreaks.Models;

/// <summary>
/// <para>Represents a class that can be used to read objects of type <see cref="LuxBreaksCandlestickMap"/> from .csv files that https://www.tradingview.com exports</para>
/// <para>Can be used for both reading and writing and uses <see cref="CultureInfo.InvariantCulture"/></para>
/// </summary>
public class LuxBreaksCandlestickMap : ClassMap<LuxBreaksCandlestick>
{
    protected static DateTime ParseDateTime(ConvertFromStringArgs args)
    {
        string string_to_parse = args.Row.GetField("time")!;

        if (long.TryParse(string_to_parse, out long seconds_from_epoch))
            return new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(seconds_from_epoch);

        if (DateTime.TryParse(string_to_parse, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsed))
            return parsed;

        throw new ReaderException(args.Row.Context, $"The specified string \"{string_to_parse}\" was not recognized as a valid {nameof(CultureInfo.InvariantCulture)} {nameof(DateTime)} string representation", new FormatException("The string was not recognized as a valid DateTime"));
    }

    public LuxBreaksCandlestickMap()
	{
        this.Map(candlestick => candlestick.Date).Name("time").Convert(args => ParseDateTime(args));

        this.Map(candlestick => candlestick.Open).Name("open").Convert(args => decimal.Parse(args.Row.GetField("open")!, CultureInfo.InvariantCulture));
        this.Map(candlestick => candlestick.Close).Name("close").Convert(args => decimal.Parse(args.Row.GetField("close")!, CultureInfo.InvariantCulture));
        this.Map(candlestick => candlestick.High).Name("high").Convert(args => decimal.Parse(args.Row.GetField("high")!, CultureInfo.InvariantCulture));
        this.Map(candlestick => candlestick.Low).Name("low").Convert(args => decimal.Parse(args.Row.GetField("low")!, CultureInfo.InvariantCulture));
        
        this.Map(candlestick => candlestick.Upper).Name("Upper").Convert(args => decimal.Parse(args.Row.GetField("Upper")!, CultureInfo.InvariantCulture));
        this.Map(candlestick => candlestick.Lower).Name("Lower").Convert(args => decimal.Parse(args.Row.GetField("Lower")!, CultureInfo.InvariantCulture));

        //// //// ////

        this.Map(candlestick => candlestick.Date).Index(0).Name("time").Convert(args => args.Value.Date.ToString(CultureInfo.InvariantCulture));

        this.Map(candlestick => candlestick.Open).Index(1).Name("open").Convert(args => args.Value.Open.ToString(CultureInfo.InvariantCulture));
        this.Map(candlestick => candlestick.Close).Index(2).Name("close").Convert(args => args.Value.Close.ToString(CultureInfo.InvariantCulture));
        this.Map(candlestick => candlestick.High).Index(3).Name("high").Convert(args => args.Value.High.ToString(CultureInfo.InvariantCulture));
        this.Map(candlestick => candlestick.Low).Index(4).Name("low").Convert(args => args.Value.Low.ToString(CultureInfo.InvariantCulture));
        
        this.Map(candlestick => candlestick.Upper).Index(5).Name("Upper").Convert(args => args.Value.Upper.ToString(CultureInfo.InvariantCulture));
        this.Map(candlestick => candlestick.Lower).Index(5).Name("Lower").Convert(args => args.Value.Lower.ToString(CultureInfo.InvariantCulture));
    }
}
