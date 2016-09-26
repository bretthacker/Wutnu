CREATE TABLE [dbo].[ErrorLogs] (
    [ErrorId]               INT            IDENTITY (1, 1) NOT NULL,
    [ErrorDate]             DATETIME       NOT NULL,
    [ErrorMessage]          NVARCHAR (MAX) NULL,
    [CallStack]             NVARCHAR (MAX) NULL,
    [UserAgent]             NVARCHAR (MAX) NULL,
    [IPAddress]             NVARCHAR (MAX) NULL,
    [UserComment]           NVARCHAR (MAX) NULL,
    [Email]                 NVARCHAR (MAX) NULL,
    [ValidationErrors]      NVARCHAR (MAX) NULL,
    [ErrorSource]           NVARCHAR (MAX) NULL,
    [StackTrace]            NVARCHAR (MAX) NULL,
    [InnerExceptionSource]  NVARCHAR (MAX) NULL,
    [QSData]                NVARCHAR (MAX) NULL,
    [PostData]              NVARCHAR (MAX) NULL,
    [Referrer]              NVARCHAR (MAX) NULL,
    [Status]                NVARCHAR (MAX) NULL,
    [UserName]              NVARCHAR (MAX) NULL,
    [InnerExceptionMessage] NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_dbo.ErrorLogs] PRIMARY KEY CLUSTERED ([ErrorId] ASC)
);

