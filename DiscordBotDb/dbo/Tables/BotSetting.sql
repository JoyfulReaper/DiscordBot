﻿CREATE TABLE [dbo].[BotSetting]
(
	[BotSettingId] BIGINT NOT NULL PRIMARY KEY IDENTITY, 
    [Token] VARCHAR(71) NULL, 
    [Game] VARCHAR(100) NULL
)
