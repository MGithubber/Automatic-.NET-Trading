CREATE PROCEDURE [dbo].[spAddCandlestick]
	@CurrencyPair VARCHAR(8),
	@DateTime DATETIME,
	@Open DECIMAL(18, 4),
	@High DECIMAL(18, 4),
	@Low DECIMAL(18, 4),
	@Close DECIMAL(18, 4),
	
	@ScopeIdentity INT OUTPUT
AS
BEGIN
	IF EXISTS (SELECT [Currency Pair], [DateTime] FROM [Candlesticks] WHERE @CurrencyPair = [Currency Pair] AND [DateTime] = @DateTime)
		BEGIN
			SET @ScopeIdentity = 0
			RETURN
		END
	
	INSERT INTO [Candlesticks] VALUES (@CurrencyPair, @DateTime, @Open, @High, @Low, @Close)
	SET @ScopeIdentity =  CAST(scope_identity() AS INT)
END
