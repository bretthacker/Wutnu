CREATE TABLE [dbo].[User] (
    [UserId]       INT            IDENTITY (1, 1) NOT NULL,
    [UserOID]      NVARCHAR (128) NULL,
    [PrimaryEmail] NVARCHAR (128) NULL,
    [ExtClaims]    VARCHAR (MAX)  NULL,
    [ApiKey]       NVARCHAR (128) NULL,
    [iss]          NVARCHAR (255) NULL,
    [idp]          NVARCHAR (255) NULL,
    PRIMARY KEY CLUSTERED ([UserId] ASC)
);




GO
CREATE NONCLUSTERED INDEX [IX_User_ApiKey]
    ON [dbo].[User]([ApiKey] ASC);

