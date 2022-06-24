CREATE TABLE [dbo].[Warning]
(
	[WarningId] BIGINT NOT NULL PRIMARY KEY IDENTITY, 
    [UserId] BIGINT NOT NULL, 
    [GuildId] BIGINT NOT NULL, 
    [Text] NVARCHAR(1000) NULL, 
    [DateCleared] DATETIME2 NULL , 
    [DateCreated] DATETIME2 NOT NULL DEFAULT GETUTCDATE(), 
    CONSTRAINT [FK_Warning_User] FOREIGN KEY ([UserId]) REFERENCES [User]([UserId]), 
    CONSTRAINT [FK_Warning_Guild] FOREIGN KEY ([GuildId]) REFERENCES [Guild]([GuildId])
)
