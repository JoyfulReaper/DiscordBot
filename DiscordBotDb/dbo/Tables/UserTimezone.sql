CREATE TABLE [dbo].[UserTimezone]
(
	[UserTimezoneId] BIGINT NOT NULL PRIMARY KEY IDENTITY, 
    [UserId] BIGINT NOT NULL, 
    [Timezone] VARCHAR(500) NOT NULL, 
    [DateCreated] DATETIME2 NOT NULL DEFAULT GETUTCDATE(), 
    CONSTRAINT [FK_UserTimezone_UserId] FOREIGN KEY ([UserId]) REFERENCES [User]([UserId])
)
