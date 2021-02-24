SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

GO
PRINT N'Creating [dbo].[DetectedEmailList]...';
GO
CREATE TYPE [dbo].[DetectedEmailList] AS TABLE (
    [MessageId] VARCHAR (100)  NOT NULL,
    [Subject]   NVARCHAR (255) NOT NULL,
    [From]      VARCHAR (255)  NULL,
    [To]        VARCHAR (255)  NULL,
    [Date]      VARCHAR (50)   NOT NULL);


GO
PRINT N'Creating [dbo].[DetectedMessages]...';


GO
CREATE TABLE [dbo].[DetectedMessages] (
    [ID]        INT            IDENTITY (1, 1) NOT NULL,
    [MessageId] VARCHAR (100)  NOT NULL,
    [Subject]   NVARCHAR (255) NULL,
    [From]      VARCHAR (255)  NULL,
    [To]        VARCHAR (255)  NULL,
    [Date]      VARCHAR (50)   NULL,
    CONSTRAINT [DetectedMessagesId] PRIMARY KEY CLUSTERED ([ID] ASC)
);


GO
PRINT N'Creating [dbo].[DetectedMessageAdd]...';


GO
CREATE PROCEDURE [dbo].[DetectedMessageAdd]
    (
      @MessageID  VARCHAR(100),
      @Subject    NVARCHAR(255),
      @From       VARCHAR(255),
      @To         VARCHAR(255),
      @Date       VARCHAR(50)
    )
AS
    INSERT INTO [dbo].[DetectedMessages] (MessageId, [Subject], [From], [To], [Date])
        VALUES (@MessageID, @Subject, @From, @To, @Date)
GO
PRINT N'Creating [dbo].[DetectedMessageAddRange]...';


GO
CREATE PROCEDURE [dbo].[DetectedMessageAddRange]
    @EmailList [DetectedEmailList] READONLY 
AS
    INSERT INTO [dbo].[DetectedMessages]
        SELECT * FROM @EmailList
