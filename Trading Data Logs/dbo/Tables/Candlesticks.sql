CREATE TABLE [dbo].[Candlesticks]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY,
    [Currency Pair] VARCHAR(8) NOT NULL,
	[DateTime] DATETIME NOT NULL, 
    [Open] DECIMAL(18, 4) NOT NULL, 
    [High] DECIMAL(18, 4) NOT NULL, 
    [Low] DECIMAL(18, 4) NOT NULL, 
    [Close] DECIMAL(18, 4) NOT NULL,
)
