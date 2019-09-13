-- =============================================
-- Author:		Brett Hacker
-- Create date: 5/16/2016
-- Description:	Take audit data and create history record
-- =============================================
CREATE PROCEDURE usp_AddHistory 
	@ShortUrl nvarchar(128), 
	@CallDate datetime2,
	@UserId int,
	@HostIp VARCHAR(64),
    @Latitude VARCHAR(50), 
    @Longitude VARCHAR(50), 
    @City NVARCHAR(100), 
    @Region NVARCHAR(100), 
    @Country NVARCHAR(50), 
    @Continent NVARCHAR(50), 
    @Isp NVARCHAR(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	DECLARE @WutLinkId INT

	SELECT @WutLinkId = WutLinkId
	FROM WutLink
	WHERE ShortUrl = @ShortUrl

	INSERT UrlHistory(
		WutLinkId, 
		CallDate, 
		UserId,
		HostIp, 
		Latitude, 
		Longitude, 
		City, 
		Region, 
		Country, 
		Continent, 
		Isp
	)
	VALUES (
		@WutLinkId, 
		@CallDate, 
		@UserId,
		@HostIp, 
		@Latitude,
		@Longitude, 
		@City, 
		@Region, 
		@Country, 
		@Continent, 
		@Isp
	)
END