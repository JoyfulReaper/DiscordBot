CREATE PROCEDURE [dbo].[spUser_Upsert]
	@UserId BIGINT,
	@DiscordUserId DECIMAL(20,0),
	@UserName NVARCHAR(100)
AS
BEGIN
	BEGIN TRANSACTION;
 
	INSERT dbo.[User]
		(
			DiscordUserId,
			UserName
		) 
	VALUES
		(
			@DiscordUserId,
			@UserName
		)

 
	IF @@ROWCOUNT = 0
	BEGIN
	  UPDATE dbo.[User]
		SET 
			UserName = @UserName
	END
	ELSE
		SET @UserId = SCOPE_IDENTITY();
 
	COMMIT TRANSACTION;

	SELECT @UserId;
END