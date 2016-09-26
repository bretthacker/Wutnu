CREATE TABLE [dbo].[Report] (
    [ReportId]    INT            IDENTITY (1, 1) NOT NULL,
    [ReportName]  NVARCHAR (50)  NOT NULL,
    [ReportPath]  NVARCHAR (255) NOT NULL,
    [Description] NVARCHAR (MAX) NULL,
    [CreateDate]  DATETIME2 (7)  NULL,
    PRIMARY KEY CLUSTERED ([ReportId] ASC)
);

