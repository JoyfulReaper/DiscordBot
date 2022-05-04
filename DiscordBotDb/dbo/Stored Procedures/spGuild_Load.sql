CREATE PROCEDURE [dbo].[spGuild_Load]
	@DiscordGuildId BIGINT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[GuildId], 
		[DiscordGuildId],
		[WelcomeUsers],
		[LoggingChannel],
		[WelcomeChannel],
		[WelcomeBackground],
		[EmbedColor],
		[AllowInvites], 
		[Prefix],
		[DateCreated]
	FROM
	[dbo].[Guild]
	WHERE
		[DiscordGuildId] = @DiscordGuildId
END	