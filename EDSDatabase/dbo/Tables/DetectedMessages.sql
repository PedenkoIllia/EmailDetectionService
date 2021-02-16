CREATE TABLE [dbo].[DetectedMessages]
(
    [ID]        INT             IDENTITY (1, 1) NOT NULL, 
    [MessageId] VARCHAR(100)    NOT NULL, 
    [Subject]   NVARCHAR(255)   NULL, 
    [From]      VARCHAR(255)    NULL, 
    [To]        VARCHAR(255)    NULL, 
    [Date]      DATETIME        NULL,

    CONSTRAINT [DetectedMessagesId] PRIMARY KEY CLUSTERED ([ID] ASC)
)
