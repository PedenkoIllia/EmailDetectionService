CREATE TYPE [dbo].[DetectedEmailList] AS TABLE(
    [MessageId] VARCHAR(100)    NOT NULL, 
    [Subject]   NVARCHAR(255)   NOT NULL, 
    [From]      VARCHAR(255)    NULL, 
    [To]        VARCHAR(255)    NULL, 
    [Date]      VARCHAR(50)     NOT NULL
)