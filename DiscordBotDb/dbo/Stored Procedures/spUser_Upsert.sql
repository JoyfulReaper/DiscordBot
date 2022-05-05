CREATE PROCEDURE [dbo].[spUser_Upsert]
	@DiscordUserId VARCHAR(20)
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