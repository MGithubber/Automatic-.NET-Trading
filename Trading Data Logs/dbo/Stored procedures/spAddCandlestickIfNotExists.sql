CREATE PROCEDURE [dbo].[spAddCandlestickIfNotExists]
	@CurrencyPair VARCHAR(8),
	@DateTime DATETIME,
	@Open DECIMAL(18, 4),
	@High DECIMAL(18, 4),
	@Low DECIMAL(18, 4),
	@Close DECIMAL(18, 4),
	@LuxAlgoSignal VARCHAR(16) = NULL
AS
BEGIN
	DECLARE @ScopeIdentity INT = 0
	SELECT TOP 1 @ScopeIdentity = [Id] FROM [Candlesticks] WHERE @CurrencyPair = [Currency Pair] AND [DateTime] = @DateTime
	
	
	IF @ScopeIdentity != 0
		RETURN @ScopeIdentity
	
	INSERT INTO [Candlesticks] VALUES (@CurrencyPair, @DateTime, @Open, @High, @Low, @Close, @LuxAlgoSignal)
	RETURN CAST(scope_identity() AS INT)
END
