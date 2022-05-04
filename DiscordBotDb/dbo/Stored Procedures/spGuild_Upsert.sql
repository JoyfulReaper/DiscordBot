CREATE PROCEDURE [dbo].[spGuild_Upsert]
	@Id BIGINT,
	@GuildId BIGINT,
	@LogginChannel BIGINT
AS
BEGIN
	BEGIN TRANSACTION;
 
	UPDATE dbo.Guild WITH (UPDLOCK, SERIALIZABLE) 
		SET 
			GuildId = @GuildId,
			LoggingChannel = @LogginChannel
		WHERE 
			[Id] = @Id;
 
	IF @@ROWCOUNT = 0
	BEGIN
	  INSERT dbo.Guild
		(GuildId,
		LoggingChannel)
	  VALUES
		(@GuildId,
		@LogginChannel);

	  SET @Id = SCOPE_IDENTITY();
	END

	COMMIT TRANSACTION;

	RETURN @Id;
END
