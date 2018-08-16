-- =============================================
-- Author:		Brett Hacker
-- Create date: 5/16/2016
-- Description:	Take audit data and create history record
-- =============================================
CREATE PROCEDURE usp_AddHistory 
	@ShortUrl nvarchar(128), 
	@CallDate datetime2,
	@UserId int,
	@HostIp varchar(64)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	DECLARE @WutLinkId int

	SELECT @WutLinkId = WutLinkId
	FROM WutLink
	WHERE ShortUrl = @ShortUrl

	INSERT UrlHistory(WutLinkId, CallDate, HostIp, UserId)
	VALUES (@WutLinkId, @CallDate, @HostIp, @UserId)
END