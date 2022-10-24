CREATE PROCEDURE [dbo].[spAddFuturesOrderAndCandlestickIfNotExists]
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
	EXEC @Candlestick_Identity = [spAddCandlestickIfNotExists] @CurrencyPair, @DateTime, @Open, @High, @Low, @Close
	EXEC @FuturesOrder_Identity = [spAddFuturesOrderIfNotExists] @Candlestick_Identity, @CurrencyPair, @BinanceID, @CreateTime, @OrderSide, @OrderType, @Price, @Quantity
END
