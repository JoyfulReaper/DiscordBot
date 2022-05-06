CREATE TABLE [dbo].[Guild]
(
	[GuildId] BIGINT NOT NULL PRIMARY KEY IDENTITY, 
    [DiscordGuildId] DECIMAL(20) NOT NULL, 
	[WelcomeUsers] BIT NOT NULL DEFAULT 0,
    [LoggingChannel] DECIMAL(20) NULL,
	[WelcomeChannel] DECIMAL(20) NULL,
	[WelcomeBackground] VARCHAR(800) NULL,
	[EmbedColor] BIGINT NULL,
	[AllowInvites] BIT NOT NULL DEFAULT 0,
	[Prefix] NVARCHAR(10) NULL,
    [DateCreated] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(), 
    CONSTRAINT [CK_Guild_DiscordGuildId] CHECK (DiscordGuildId >= 0 AND DiscordGuildId <= 18446744073709551615),
	CONSTRAINT [CK_Guild_LoggingChannel] CHECK (LoggingChannel >= 0 AND LoggingChannel <= 18446744073709551615),
	CONSTRAINT [CK_Guild_WelcomeChannel] CHECK (WelcomeChannel >= 0 AND WelcomeChannel <= 18446744073709551615)
)
