﻿CREATE TABLE [dbo].[Guild]
(
	[Id] BIGINT NOT NULL PRIMARY KEY IDENTITY, 
    [GuildId] BIGINT NOT NULL, 
    [LoggingChannel] BIGINT NULL
)
