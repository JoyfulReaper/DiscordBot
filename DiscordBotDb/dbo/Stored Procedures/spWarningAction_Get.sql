CREATE PROCEDURE [dbo].[spWarningAction_Get]
	@GuildId BIGINT
AS
BEGIN
	SET NOCOUNT ON;
	SELECT 
		[WarningActionId], 
		[GuildId], 
		[Action], 
		[ActionThreshold]
	FROM
		WarningAction
	WHERE
		GuildId = @GuildId
END
