CREATE PROCEDURE [dbo].[spAddFuturesOrderIfNotExists]
	@CandlestickID INT,
	@Symbol VARCHAR(8),
	@BinanceID BIGINT,
	@CreateTime DATETIME,
	@OrderSide VARCHAR(8),
	@OrderType VARCHAR(32),
	@Price DECIMAL(18, 4),
	@Quantity DECIMAL(18, 4)
AS
BEGIN
	DECLARE @FuturesOrderId INT = 0
	SELECT TOP 1 @FuturesOrderId = [Id] FROM [Futures orders] WHERE @BinanceID = [Binance ID]


	IF @FuturesOrderId != 0
		RETURN @FuturesOrderId
	
	INSERT INTO [Futures orders] VALUES (@CandlestickID, @Symbol, @BinanceID, @CreateTime, @OrderSide, @OrderType, @Price, @Quantity)
	RETURN CAST(scope_identity() AS INT)
END
