CREATE PROCEDURE [dbo].[spAddFuturesOrder]
	@CandlestickID INT = NULL,
	@Symbol VARCHAR(8),
	@BinanceID BIGINT,
	@CreateTime DATETIME,
	@OrderSide VARCHAR(8),
	@OrderType VARCHAR(32),
	@Price DECIMAL(18, 4),
	@Quantity DECIMAL(18, 4),

	@ScopeIdentity INT OUTPUT
AS
BEGIN
	IF @CandlestickID != NULL AND EXISTS (SELECT [Binance ID] FROM [Futures orders] WHERE @BinanceID = [Binance ID])
		BEGIN
			SET @ScopeIdentity = 0
			RETURN
		END
	
	INSERT INTO [Futures orders] VALUES (@CandlestickID, @Symbol, @BinanceID, @CreateTime, @OrderSide, @OrderType, @Price, @Quantity)
	SET @ScopeIdentity =  CAST(scope_identity() AS INT)
END
