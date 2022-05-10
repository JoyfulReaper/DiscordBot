CREATE PROCEDURE [dbo].[spUserTimezone_Upsert]
	@UserTimezoneId BIGINT,
	@UserId BIGINT,
	@Timezone VARCHAR(500)
AS
BEGIN
	BEGIN TRANSACTION;
 
	INSERT dbo.[UserTimezone]
	(
		[UserId],
		[Timezone]
	) 
	SELECT
		@UserId,
		@Timezone
	WHERE NOT EXISTS
	(
		SELECT 1 FROM [dbo].[UserTimezone] WITH (UPDLOCK, SERIALIZABLE)
		WHERE [UserTimezoneId] = @UserTimezoneId
	)

 
	IF @@ROWCOUNT = 0
	BEGIN
	  UPDATE dbo.[UserTimezone]
		SET 
			UserId = @UserId,
			Timezone = @Timezone
		WHERE
			UserTimezoneId = @UserTimezoneId
	END
	ELSE
		SET @UserTimezoneId = SCOPE_IDENTITY();
 
	COMMIT TRANSACTION;

	SELECT @UserTimezoneId;
END