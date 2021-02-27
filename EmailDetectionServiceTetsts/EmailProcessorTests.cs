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
