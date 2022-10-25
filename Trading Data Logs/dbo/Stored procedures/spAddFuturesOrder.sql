CREATE PROCEDURE [dbo].[spAddFuturesOrder]
	@CurrencyPair VARCHAR(8),
	
	@BinanceID BIGINT,
	@CreateTime DATETIME,
	@OrderSide VARCHAR(8),
	@OrderType VARCHAR(32),
	@Price DECIMAL(18, 4),
	@Quantity DECIMAL(18, 4),

	@DateTime DATETIME,
	@Open DECIMAL(18, 4),
	@High DECIMAL(18, 4),
	@Low DECIMAL(18, 4),
	@Close DECIMAL(18, 4),
	@LuxAlgoSignal VARCHAR(16) = NULL,
	
	@Candlestick_Identity INT OUTPUT,
	@FuturesOrder_Identity INT OUTPUT
AS
BEGIN
	SELECT TOP 1 @Candlestick_Identity = [Id] FROM [Candlesticks] WHERE @CurrencyPair = [Currency Pair] AND [DateTime] = @DateTime
	IF @Candlestick_Identity IS NULL
		EXEC @Candlestick_Identity = [spAddCandlestick] @CurrencyPair, @DateTime, @Open, @High, @Low, @Close, @LuxAlgoSignal

	
	INSERT INTO [Futures orders] VALUES (@Candlestick_Identity, @BinanceID, @CreateTime, @OrderSide, @OrderType, @Price, @Quantity)
	SET @FuturesOrder_Identity = CAST(SCOPE_IDENTITY() AS INT)
END
