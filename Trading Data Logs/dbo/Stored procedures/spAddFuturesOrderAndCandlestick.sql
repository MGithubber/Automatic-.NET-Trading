CREATE PROCEDURE [dbo].[spAddFuturesOrderAndCandlestick]
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
	
	@Candlestick_Identity INT OUTPUT,
	@FuturesOrder_Identity INT OUTPUT
AS
BEGIN
	IF	EXISTS (SELECT [Currency Pair], [DateTime] FROM [Candlesticks] WHERE @CurrencyPair = [Currency Pair] AND [DateTime] = @DateTime) -- checks if the candlestick exists
		OR 
		EXISTS (SELECT [Binance ID] FROM [Futures orders] WHERE @BinanceID = [Binance ID]) -- checks if the futures order exists
		BEGIN
			SET @Candlestick_Identity = 0
			SET @FuturesOrder_Identity = 0
			RETURN
		END
		
	
	EXEC spAddCandlestick @CurrencyPair, @DateTime, @Open, @High, @Low, @Close, @Candlestick_Identity OUTPUT -- adds the candlestick
	EXEC spAddFuturesOrder @Candlestick_Identity, @CurrencyPair, @BinanceID, @CreateTime, @OrderSide, @OrderType, @Price, @Quantity, @FuturesOrder_Identity OUTPUT -- adds the futures order
END
