using System.Globalization;

using CsvHelper;

using Skender.Stock.Indicators;

namespace Domain.Integration.Testing;

[TestFixture]
public class CandlestickTests
{
    private int length = 69000;
    private Quote[] Quotes { get; set; }
    private Candlestick[] Candlesticks { get; set; }


    #region private methods
    private static Candlestick[] QuoteArray_to_CandlestickArray(Quote[] Quotes)
    {
        Candlestick[] Candlesticks = new Candlestick[Quotes.Length];
        for (int i = 0; i < Candlesticks.Length; i++)
            Candlesticks[i] = new Candlestick
            {
                Date = Quotes[i].Date,
                Open = Quotes[i].Open,
                High = Quotes[i].High,
                Low = Quotes[i].Low,
                Close = Quotes[i].Close
            };
        
        return Candlesticks;
    }
    #endregion


    #region Tests
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
        this.Candlesticks = CandlestickTests.QuoteArray_to_CandlestickArray(this.Quotes);

        // Act
        List<ParabolicSarResult> PsarResult_Quotes = this.Quotes.GetParabolicSar<Quote>().ToList();
        List<ParabolicSarResult> PsarResult_Candlesticks = this.Candlesticks.GetParabolicSar<Candlestick>().ToList();

        // Assert
        PsarResult_Quotes.Should().NotBeEmpty().And.BeEquivalentTo(PsarResult_Candlesticks);
    }

    [Test, Order(2)]
    public void GetParabolicSar_RealValuesChart_Returns_Same_Results()
    {
        // Arrange
        StreamReader reader = new StreamReader(@"..\Process files\Candlestick charts\BINANCE_ETHBUSD, 15.csv");
        CsvReader csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        csv.Context.RegisterClassMap<CsvQuoteTradingviewStyleMap>();
        this.Quotes = csv.GetRecords<Quote>().ToArray();
        this.Candlesticks = CandlestickTests.QuoteArray_to_CandlestickArray(this.Quotes);

        // Act
        List<ParabolicSarResult> PsarResult_Quotes = this.Quotes.GetParabolicSar<Quote>().ToList();
        List<ParabolicSarResult> PsarResult_Candlesticks = this.Candlesticks.GetParabolicSar<Candlestick>().ToList();

        // Assert
        PsarResult_Quotes.Should().NotBeEmpty().And.BeEquivalentTo(PsarResult_Candlesticks);
    } 
    #endregion
}
