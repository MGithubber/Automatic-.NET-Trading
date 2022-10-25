CREATE PROCEDURE [dbo].[spDeleteCandlestick]
	@CurrencyPair VARCHAR(8),
	@DateTime DATETIME
AS
BEGIN
	DECLARE @Identity INT
	SELECT TOP 1 @Identity = [Id] FROM [Candlesticks] WHERE @CurrencyPair = [Currency Pair] AND [DateTime] = @DateTime

	IF @Identity IS NULL
		RETURN NULL
	
	DELETE FROM [Candlesticks] WHERE [Id] = @Identity
	RETURN @Identity
END
