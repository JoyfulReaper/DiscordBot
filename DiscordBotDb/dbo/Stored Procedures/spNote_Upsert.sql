CREATE PROCEDURE [dbo].[spNote_Upsert]
	@NoteId BIGINT,
	@UserId BIGINT,
	@Name NVARCHAR(30),
	@Text NVARCHAR(300)
AS
BEGIN
	BEGIN TRANSACTION
	
	INSERT INTO [dbo].[Note]
	(
		[UserId],
		[Name],
		[Text]
	)
	SELECT
		@UserId,
		@Name,
		@Text
	WHERE NOT EXISTS
	(
		SELECT 1 FROM [dbo].[Note] WITH (UPDLOCK, SERIALIZABLE)
		WHERE [NoteId] = @NoteId
	)
		
	IF @@ROWCOUNT = 0
	BEGIN
		UPDATE [dbo].[Note]
			SET
				[Name] = @Name,
				[Text] = @Text
			WHERE
				NoteId = @NoteId;
	END
	ELSE
		SET @NoteId = SCOPE_IDENTITY();
	
	COMMIT TRANSACTION
	
	SELECT @NoteId
END