CREATE TABLE [dbo].[WarningAction]
(
	[WarningActionId] BIGINT NOT NULL PRIMARY KEY IDENTITY, 
    [GuildId] BIGINT NOT NULL, 
    [Action] INT NOT NULL, 
    [ActionThreshold] INT NOT NULL, 
    CONSTRAINT [FK_WarningAction_Guild] FOREIGN KEY ([GuildId]) REFERENCES [Guild]([GuildId])
)
