CREATE PROCEDURE [dbo].[spWarning_Add]
	@UserId BIGINT,
	@GuildId BIGINT,
	@Text NVARCHAR(1000)
	
AS
BEGIN
	INSERT INTO Warning 
		(UserId, 
		GuildId, 
		[Text])
	VALUES
		(@UserId,
		@GuildId,
		@Text)
END
