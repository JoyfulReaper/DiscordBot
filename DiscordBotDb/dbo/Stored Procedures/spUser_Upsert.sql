CREATE PROCEDURE [dbo].[spUser_Upsert]
	@UserId BIGINT,
	@DiscordUserId DECIMAL(20,0),
	@UserName NVARCHAR(100)
AS
BEGIN
	BEGIN TRANSACTION;
 
	INSERT dbo.[User]
	(
		[DiscordUserId],
		[UserName]
	) 
	SELECT
		@DiscordUserId,
		@UserName
	WHERE NOT EXISTS
	(
		SELECT 1 FROM [dbo].[User] WITH (UPDLOCK, SERIALIZABLE)
		WHERE [UserId] = @UserId
	)

 
	IF @@ROWCOUNT = 0
	BEGIN
	  UPDATE dbo.[User]
		SET 
			UserName = @UserName
		WHERE
			[UserId] = @UserId
	END
	ELSE
		SET @UserId = SCOPE_IDENTITY();
 
	COMMIT TRANSACTION;

	SELECT @UserId;
END