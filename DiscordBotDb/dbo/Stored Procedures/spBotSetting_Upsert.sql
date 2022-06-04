CREATE PROCEDURE [dbo].[spBotSetting_Upsert]
	@BotSettingId int,
	@Token varchar(60),
	@Game varchar(100)

AS
BEGIN
	BEGIN TRANSACTION;

	UPDATE dbo.BotSetting WITH (UPDLOCK, SERIALIZABLE)
		SET
			Token = @Token,
			Game = @Game
		WHERE
			BotSettingId = @BotSettingId;

	IF @@ROWCOUNT = 0
	BEGIN
		INSERT dbo.BotSetting
		(
			Token,
			Game
		)
		VALUES
		(
			@Token,
			@Game
		)

		SET @BotSettingId = SCOPE_IDENTITY();
	END

	COMMIT TRANSACTION
END
