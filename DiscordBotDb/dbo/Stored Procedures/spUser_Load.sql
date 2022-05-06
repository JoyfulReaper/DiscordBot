CREATE PROCEDURE [dbo].[spUser_Load]
	@DiscordUserId DECIMAL(20,0)
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