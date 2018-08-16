CREATE TABLE [dbo].[UrlHistory] (
    [HistoryId] BIGINT       IDENTITY (1, 1) NOT NULL,
    [WutLinkId] INT          NOT NULL,
    [CallDate]  DATETIME     NOT NULL,
    [HostIp]    VARCHAR (64) NOT NULL,
    [UserId]    INT          NULL,
    CONSTRAINT [PK_UrlHistories] PRIMARY KEY CLUSTERED ([HistoryId] ASC),
    CONSTRAINT [FK_UrlHistories_ShortUrls] FOREIGN KEY ([WutLinkId]) REFERENCES [dbo].[WutLink] ([WutLinkId]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_UrlHistories]
    ON [dbo].[UrlHistory]([CallDate] ASC, [HostIp] ASC, [UserId] ASC);

