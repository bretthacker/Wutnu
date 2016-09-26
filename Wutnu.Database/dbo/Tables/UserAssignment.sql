CREATE TABLE [dbo].[UserAssignment] (
    [UserAssignmentId] BIGINT         IDENTITY (1, 1) NOT NULL,
    [WutLinkId]        INT            NOT NULL,
    [UserEmail]        NVARCHAR (128) NOT NULL,
    [UserId]           INT            NULL,
    CONSTRAINT [PK_UserAssignments] PRIMARY KEY CLUSTERED ([UserAssignmentId] ASC),
    CONSTRAINT [FK_UserAssignments_ShortUrls] FOREIGN KEY ([WutLinkId]) REFERENCES [dbo].[WutLink] ([WutLinkId]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_UserAssignments]
    ON [dbo].[UserAssignment]([UserEmail] ASC, [UserId] ASC);

