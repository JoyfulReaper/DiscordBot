﻿CREATE TABLE [dbo].[BotSetting]
(
	[BotSettingId] BIGINT NOT NULL PRIMARY KEY IDENTITY, 
    [Token] VARCHAR(60) NULL, 
    [Game] VARCHAR(100) NULL
)
