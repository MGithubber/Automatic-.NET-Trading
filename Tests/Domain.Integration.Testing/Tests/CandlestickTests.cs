using System.Globalization;

using CsvHelper;
using Domain.Integration.Testing;

using Skender.Stock.Indicators;

namespace Domain.Tests.Tests;

[TestFixture]
public class CandlestickTests
{
    private int length = 69000;
    private IEnumerable<Quote> Quotes = default!;
    private IEnumerable<Candlestick> Candlesticks = default!;


    [Test, Order(1)]
    public void GetParabolicSar_RandomValuesChart_Returns_Same_Results()
    {
        // Arrange
        Random random = new Random();
        this.Quotes = Enumerable.Range(0, this.length).Select(_ => new Quote
        {
            Date = DateTime.MinValue,
            Open = random.Next(800, 1200) + (decimal)Math.Round(random.NextDouble(), 4),
            High = random.Next(800, 1200) + (decimal)Math.Round(random.NextDouble(), 4),
            Low = random.Next(800, 1200) + (decimal)Math.Round(random.NextDouble(), 4),
            Close = random.Next(800, 1200) + (decimal)Math.Round(random.NextDouble(), 4)
        }).ToArray();
        this.Candlesticks = this.Quotes.Select(quote => new Candlestick
        {
            Date = quote.Date,
            Open = quote.Open,
            High = quote.High,
            Low = quote.Low,
            Close = quote.Close
        });

        // Act
        var PsarResult_Quotes = this.Quotes.GetParabolicSar();
        var PsarResult_Candlesticks = this.Candlesticks.GetParabolicSar();

        // Assert
        PsarResult_Quotes.Should().NotBeEmpty().And.BeEquivalentTo(PsarResult_Candlesticks);
    }

    [Test, Order(2)]
    public void GetParabolicSar_RealValuesChart_Returns_Same_Results()
    {
        // Arrange
        StreamReader reader = new StreamReader(@"..\..\Process files\Candlestick charts\BINANCE_ETHBUSD, 15.csv");
        CsvReader csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        csv.Context.RegisterClassMap<CsvQuoteTradingviewStyleMap>();
        this.Quotes = csv.GetRecords<Quote>().ToArray();
        this.Candlesticks = this.Quotes.Select(quote => new Candlestick
        {
            Date = quote.Date,
            Open = quote.Open,
            High = quote.High,
            Low = quote.Low,
            Close = quote.Close
        });
        
        // Act
        var PsarResult_Quotes = this.Quotes.GetParabolicSar();
        var PsarResult_Candlesticks = this.Candlesticks.GetParabolicSar();

        // Assert
        PsarResult_Quotes.Should().NotBeEmpty().And.BeEquivalentTo(PsarResult_Candlesticks);
    }
}
