CREATE PROCEDURE [dbo].[DetectedMessageAddRange]
    @EmailList [DetectedEmailList] READONLY 
AS
    INSERT INTO [dbo].[DetectedMessages]
        SELECT * FROM @EmailList
