CREATE TABLE [dbo].[Futures orders]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [Candlestick Id] INT NOT NULL,
    -- [Symbol] VARCHAR(8) NOT NULL, 
    [Binance ID] BIGINT NOT NULL, 
    [CreateTime] DATETIME NOT NULL, 
    [Order side] VARCHAR(8) NOT NULL, 
    [Order type] VARCHAR(32) NOT NULL,
    [Price] DECIMAL(18, 4) NOT NULL, 
    [Quantity] DECIMAL(18, 4) NOT NULL, 
    
    CONSTRAINT UniqueBinanceID UNIQUE ([Binance ID]),
    -- CONSTRAINT UniqueFuturesOrder UNIQUE ([Symbol], [CreateTime]),

    CONSTRAINT [FK_Futures_orders_ToCandlestick] FOREIGN KEY ([Candlestick Id]) REFERENCES [Candlesticks]([Id])
)
