CREATE PROCEDURE [dbo].[spGuild_Upsert]
	@GuildId BIGINT,
	@DiscordGuildId DECIMAL(20,0),
	@WelcomeUsers BIT,
	@LoggingChannel DECIMAL(20,0),
	@WelcomeChannel DECIMAL(20,0),
	@WelcomeBackground VARCHAR(800),
	@EmbedColor BIGINT,
	@AllowInvites BIT,
	@Prefix NVARCHAR(10)
AS
BEGIN
	BEGIN TRANSACTION;
 
	UPDATE dbo.Guild WITH (UPDLOCK, SERIALIZABLE) 
		SET 
			DiscordGuildId = @DiscordGuildId,
			WelcomeUsers = @WelcomeUsers,
			LoggingChannel = @LoggingChannel,
			WelcomeChannel = @WelcomeChannel,
			WelcomeBackground = @WelcomeBackground,
			EmbedColor = @EmbedColor,
			AllowInvites = @AllowInvites,
			Prefix = @Prefix
		WHERE 
			[GuildId] = @GuildId;
 
	IF @@ROWCOUNT = 0
	BEGIN
	  INSERT dbo.Guild
		(DiscordGuildId,
		WelcomeUsers,
		LoggingChannel,
		WelcomeChannel,
		WelcomeBackground,
		EmbedColor,
		AllowInvites,
		Prefix)
	  VALUES
		(@DiscordGuildId,
		@WelcomeUsers,
		@LoggingChannel,
		@WelcomeChannel,
		@WelcomeBackground,
		@EmbedColor,
		@AllowInvites,
		@Prefix);

	  SET @GuildId = SCOPE_IDENTITY();
	END

	COMMIT TRANSACTION;

	SELECT @GuildId;
END
