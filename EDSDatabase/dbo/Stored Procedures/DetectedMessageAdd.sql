CREATE PROCEDURE [dbo].[DetectedMessageAdd]
    (
      @MessageID  VARCHAR(100),
      @Subject    NVARCHAR(255),
      @From       VARCHAR(255),
      @To         VARCHAR(255),
      @Date       DATETIME
    )
AS
    INSERT INTO [dbo].[DetectedMessages] (MessageId, [Subject], [From], [To], [Date])
        VALUES (@MessageID, @Subject, @From, @To, @Date)
