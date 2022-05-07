CREATE PROCEDURE [dbo].[spNote_LoadAll]
	@UserId BIGINT
AS
BEGIN
	SET NOCOUNT ON
	
	SELECT
		[NoteId],
		[UserId],
		[Name],
		[Text]
	FROM
		[dbo].[Note]
	WHERE
		[UserId] = @UserId
	AND [Deleted] = 0;
END