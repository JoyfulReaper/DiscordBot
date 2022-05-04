CREATE PROCEDURE [dbo].[spUser_Upsert]
	@DiscordUserId BIGINT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[UserId],
		[DiscordUserId],
		[UserName],
		[DateCreated]
	FROM
		[dbo].[User]
	WHERE
		DiscordUserId = @DiscordUserId;
END