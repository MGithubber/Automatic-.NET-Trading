using System.Reflection;

using Skender.Stock.Indicators;

namespace Domain.Integration.Testing;

[TestFixture]
public class CandlestickTests
{
    private int length = 69000;
    private Quote[] Quotes { get; set; }
    private Candlestick[] Candlesticks { get; set; }

    [SetUp]
    public void Setup()
    {
        Random random = new Random();

        this.Quotes = Enumerable.Range(0, this.length).Select(_ => new Quote
        {
            Date = DateTime.MinValue,
            Open = random.Next(800, 1200) + (decimal)Math.Round(random.NextDouble(), 4),
            High = random.Next(800, 1200) + (decimal)Math.Round(random.NextDouble(), 4),
            Low = random.Next(800, 1200) + (decimal)Math.Round(random.NextDouble(), 4),
            Close = random.Next(800, 1200) + (decimal)Math.Round(random.NextDouble(), 4)
        }).ToArray();

        this.Candlesticks = new Candlestick[this.length];
        for (int i = 0; i < this.length; i++)
            this.Candlesticks[i] = new Candlestick
            {
                Date = this.Quotes[i].Date,
                Open = this.Quotes[i].Open,
                High = this.Quotes[i].High,
                Low = this.Quotes[i].Low,
                Close = this.Quotes[i].Close
            };
    }

    [Test, Order(1)]
    public void GetParabolicSar_Returns_Same_Results()
    {
        // Arrange
        // Act
        List<ParabolicSarResult> PsarResult_Quotes = this.Quotes.GetParabolicSar<Quote>().ToList();
        List<ParabolicSarResult> PsarResult_Candlesticks = this.Candlesticks.GetParabolicSar<Candlestick>().ToList();
        
        // Assert
        PsarResult_Quotes.Should().NotBeEmpty().And.BeEquivalentTo(PsarResult_Candlesticks);
    }
}
