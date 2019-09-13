CREATE VIEW [dbo].[vwHistoryReport]
AS
SELECT DISTINCT 
	dbo.WutLink.UserId, 
	FORMAT(dbo.UrlHistory.CallDate, 'dd/MM/yyyy', 'en-us') AS HitDate, 
	dbo.WutLink.ShortUrl, 
	dbo.WutLink.RealUrl, 
	COUNT(dbo.UrlHistory.HistoryId) AS NumHits, 
	dbo.WutLink.IsAzureBlob, 
    dbo.WutLink.IsProtected,
	dbo.UrlHistory.Latitude,
	dbo.UrlHistory.Longitude
FROM
	dbo.UrlHistory INNER JOIN dbo.WutLink ON dbo.UrlHistory.WutLinkId = dbo.WutLink.WutLinkId
GROUP BY 
	dbo.WutLink.ShortUrl, 
	dbo.WutLink.RealUrl, 
	dbo.WutLink.IsAzureBlob, 
	FORMAT(dbo.UrlHistory.CallDate, 'dd/MM/yyyy', 'en-us'), 
	dbo.WutLink.IsProtected, 
	dbo.WutLink.UserId,
	dbo.UrlHistory.Latitude,
	dbo.UrlHistory.Longitude
GO


