CREATE PROCEDURE [dbo].[spWarning_Get]
	@UserId bigint,
	@GuildId bigint
AS
BEGIN
	SELECT
		[WarningId], 
		[UserId], 
		[GuildId], 
		[Text], 
		[DateCleared], 
		[DateCreated]
	FROM
		Warning
	WHERE
		UserId = @UserId
	AND GuildId = @GuildId
	AND DateCleared IS NULL
END
