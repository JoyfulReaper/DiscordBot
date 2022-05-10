CREATE PROCEDURE [dbo].[spUserTimezone_Load]
	@UserId BIGINT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[UserTimezoneId],
		[UserId],
		[Timezone],
		[DateCreated]
	FROM
		[dbo].[UserTimezone]
	WHERE
		[UserId] = @UserId
END