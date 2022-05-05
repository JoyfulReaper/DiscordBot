CREATE TABLE [dbo].[Guild]
(
	[GuildId] BIGINT NOT NULL PRIMARY KEY IDENTITY, 
    [DiscordGuildId] VARCHAR(20) NOT NULL, 
	[WelcomeUsers] BIT NOT NULL DEFAULT 0,
    [LoggingChannel] VARCHAR(20) NULL,
	[WelcomeChannel] VARCHAR(20) NULL,
	[WelcomeBackground] VARCHAR(800) NULL,
	[EmbedColor] VARCHAR(12) NULL,
	[AllowInvites] BIT NOT NULL DEFAULT 0,
	[Prefix] NVARCHAR(10) NULL,
    [DateCreated] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
)
