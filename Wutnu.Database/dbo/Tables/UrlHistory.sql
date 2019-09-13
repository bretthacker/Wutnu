CREATE TABLE [dbo].[UrlHistory] (
    [HistoryId] BIGINT       IDENTITY (1, 1) NOT NULL,
    [WutLinkId] INT          NOT NULL,
    [CallDate]  DATETIME     NOT NULL,
    [HostIp]    VARCHAR (64) NOT NULL,
    [UserId]    INT          NULL,
    [Latitude] VARCHAR(50) NULL, 
    [Longitude] VARCHAR(50) NULL, 
    [City] NVARCHAR(100) NULL, 
    [Region] NVARCHAR(100) NULL, 
    [Country] NVARCHAR(50) NULL, 
    [Continent] NVARCHAR(50) NULL, 
    [Isp] NVARCHAR(50) NULL, 
    CONSTRAINT [PK_UrlHistories] PRIMARY KEY CLUSTERED ([HistoryId] ASC),
    CONSTRAINT [FK_UrlHistories_ShortUrls] FOREIGN KEY ([WutLinkId]) REFERENCES [dbo].[WutLink] ([WutLinkId]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_UrlHistories]
    ON [dbo].[UrlHistory]([CallDate] ASC, [HostIp] ASC, [UserId] ASC);

