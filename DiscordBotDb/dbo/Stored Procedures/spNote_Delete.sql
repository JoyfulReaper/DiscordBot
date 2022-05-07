CREATE PROCEDURE [dbo].[spNote_Delete]
	@NoteId BIGINT
AS
BEGIN
	UPDATE [dbo].[Note]
		SET [Deleted] = 1
	WHERE
		[NoteId] = @NoteId
END
