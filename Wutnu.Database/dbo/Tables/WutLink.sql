CREATE TABLE [dbo].[WutLink] (
    [WutLinkId]   INT            IDENTITY (1, 1) NOT NULL,
    [ShortUrl]    NVARCHAR (128) NOT NULL,
    [CreateDate]  DATETIME       NOT NULL,
    [CreatedByIp] VARCHAR (64)   NULL,
    [IsProtected] BIT            NOT NULL,
    [RealUrl]     NVARCHAR (MAX) NOT NULL,
    [Comments]    NVARCHAR (MAX) NULL,
    [UserId]      INT            NULL,
    [IsAzureBlob] BIT            DEFAULT ((0)) NOT NULL,
    [UseDelay] BIT NOT NULL DEFAULT ((0)), 
    CONSTRAINT [PK_ShortUrls] PRIMARY KEY CLUSTERED ([WutLinkId] ASC),
    CONSTRAINT [FK_WutLink_User] FOREIGN KEY ([UserId]) REFERENCES [dbo].[User] ([UserId])
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_ShortUrls]
    ON [dbo].[WutLink]([ShortUrl] ASC);

