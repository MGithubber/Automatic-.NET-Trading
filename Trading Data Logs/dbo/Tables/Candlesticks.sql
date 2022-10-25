CREATE TABLE [dbo].[Candlesticks]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY,
    [Currency Pair] VARCHAR(8) NOT NULL,
	[DateTime] DATETIME NOT NULL, 
    [Open] DECIMAL(18, 4) NOT NULL, 
    [High] DECIMAL(18, 4) NOT NULL, 
    [Low] DECIMAL(18, 4) NOT NULL, 
    [Close] DECIMAL(18, 4) NOT NULL, 
    [LuxAlgoSignal] VARCHAR(16) NULL,
    
    CONSTRAINT CK_CorrectSignalString CHECK ([LuxAlgoSignal] IN('Hold', 'Buy', 'StrongBuy', 'Sell', 'StrongSell', 'ExitBuy', 'ExitSell')),
    CONSTRAINT UK_Candle UNIQUE ([Currency Pair], [DateTime])
) 
