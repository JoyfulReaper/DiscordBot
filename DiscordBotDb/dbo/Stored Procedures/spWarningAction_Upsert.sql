CREATE PROCEDURE [dbo].[spWarningAction_Upsert]
	@WarningActionId BIGINT,
	@GuildId BIGINT,
	@Action INT,
	@ActionThreshold INT
AS
BEGIN
	BEGIN TRANSACTION;

	UPDATE dbo.WarningAction WITH (UPDLOCK, SERIALIZABLE)
	SET
		[Action] = @Action,
		ActionThreshold = @ActionThreshold
	WHERE
		GuildId = @GuildId;

	IF @@ROWCOUNT = 0
	BEGIN
		INSERT dbo.WarningAction
			(GuildId,
			[Action],
			ActionThreshold)
		VALUES
			(@GuildId,
			@Action,
			@ActionThreshold);

		SET @WarningActionId = SCOPE_IDENTITY();
	END

	COMMIT TRANSACTION;

	SELECT @WarningActionId;
END
