CREATE PROCEDURE [dbo].[spBotSetting_Load]
	
AS
BEGIN
	
	SET NOCOUNT ON;
	
	SELECT
		[BotSettingId], 
		[Token], 
		[Game]
	
	FROM
		BotSetting

END
