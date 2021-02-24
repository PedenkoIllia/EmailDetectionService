using EmailDetectionService.DAL;
using EmailDetectionService.Models;
using EmailDetectionService.Processors;
using Moq;
using System;
using System.IO;
using Xunit;

namespace EmailDetectionServiceTetsts
{
    public class EmailProcessorTest
    {
        private Mock<IDataSourceDetectedMessages> moqDataSource;
        public EmailProcessorTest()
        {
            moqDataSource = new Mock<IDataSourceDetectedMessages>();
        }

        [Theory]
        [InlineData(@".\TestData\Emails\Sample1.eml")]
        [InlineData(@".\TestData\Emails\Sample2.eml")]
        [InlineData(@".\TestData\Emails\Sample3.eml")]
        public void ParseMessageFromFileTest_NormalFile(string emailFileName)
        {
            EmailProcessor processor = new EmailProcessor(moqDataSource.Object);

            MessageModel msg = processor.GetMessageModelFromFile(emailFileName);

            Assert.True(msg.Message_ID != null && msg.Subject != null && msg.From != null && msg.To != null && msg.Date != null);
        }

        [Theory]
        [InlineData(@".\TestData\PseudoEmails\Excel.eml")]
        [InlineData(@".\TestData\PseudoEmails\Notepad.eml")]
        public void ParseMessageFromFileTest_ThrowFormatException(string emailFileName)
        {
            EmailProcessor processor = new EmailProcessor(moqDataSource.Object);

            Assert.Throws<FormatException>(() => processor.GetMessageModelFromFile(emailFileName));
        }

        [Fact]
        public void ProcessMessageTest_NormalFile()
        {
            string resultFile = @".\TestData\Emails\Sample_tmp.eml";
            moqDataSource.Setup(ds => ds.InsertMessage(It.IsAny<MessageModel>())).Returns(true);
            EmailProcessor processor = new EmailProcessor(moqDataSource.Object);
            File.Copy(@".\TestData\Emails\Sample2.eml", resultFile);

            bool result = false || processor.ProcessMessage(resultFile);
            if (File.Exists(resultFile))
                File.Delete(resultFile);

            Assert.True(result);
        }

        [Fact]
        public void ProcessMessageTest_NotExistFile()
        {
            moqDataSource.Setup(ds => ds.InsertMessage(It.IsAny<MessageModel>())).Returns(true);
            EmailProcessor processor = new EmailProcessor(moqDataSource.Object);

            bool result = true && processor.ProcessMessage(@".\TestData\Emails\NoSample.eml");

            Assert.False(result);
        }
    }
}
