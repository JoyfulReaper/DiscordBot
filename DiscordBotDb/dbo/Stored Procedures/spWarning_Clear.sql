CREATE PROCEDURE [dbo].[spWarning_Clear]
	@UserId bigint,
	@GuildId bigint

AS
BEGIN
	UPDATE Warning
		SET DateCleared = GETUTCDATE()
	WHERE
		UserId = @UserId
	AND GuildId = @GuildId
	AND DateCleared IS NULL;
END
