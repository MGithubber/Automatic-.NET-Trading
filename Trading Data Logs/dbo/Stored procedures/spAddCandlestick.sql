CREATE PROCEDURE [dbo].[spAddCandlestick]
	@CurrencyPair VARCHAR(8),
	@DateTime DATETIME,
	@Open DECIMAL(18, 4),
	@High DECIMAL(18, 4),
	@Low DECIMAL(18, 4),
	@Close DECIMAL(18, 4),
	@LuxAlgoSignal VARCHAR(16) = NULL
AS
BEGIN
	INSERT INTO [Candlesticks] VALUES (@CurrencyPair, @DateTime, @Open, @High, @Low, @Close, @LuxAlgoSignal)
	RETURN CAST(SCOPE_IDENTITY() AS INT)
END
