CREATE PROCEDURE [dbo].[spDeleteFuturesOrder]
	@BinanceID BIGINT
AS
BEGIN
	DECLARE @Identity INT
	SELECT TOP 1 @Identity = [Id] FROM [Futures orders] WHERE [Binance ID] = @BinanceID

	IF @Identity IS NULL
		RETURN NULL
	
	DELETE FROM [Futures orders] WHERE [Id] = @Identity
	RETURN @Identity
END
