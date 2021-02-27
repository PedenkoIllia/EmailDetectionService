using EmailDetectionService.Helper;
using EmailDetectionService.Models;
using System;
using Xunit;

namespace EmailDetectionServiceTetsts
{
    public class MessageParserTests
    {
        [Theory]
        [InlineData(@".\TestData\Emails\Sample1.eml")]
        [InlineData(@".\TestData\Emails\Sample2.eml")]
        [InlineData(@".\TestData\Emails\Sample3.eml")]
        public void ParseMessageFromFileTest_NormalFile(string emailFileName)
        {
            MessageModel msg = MessageParser.GetMessageModelFromFile(emailFileName);

            Assert.True(msg.Message_ID != null && msg.Subject != null && msg.From != null && msg.To != null && msg.Date != null);
        }

        [Theory]
        [InlineData(@".\TestData\PseudoEmails\Excel.eml")]
        [InlineData(@".\TestData\PseudoEmails\Notepad.eml")]
        public void ParseMessageFromFileTest_ThrowFormatException(string emailFileName)
        {
            Assert.Throws<FormatException>(() => MessageParser.GetMessageModelFromFile(emailFileName));
        }
    }
}
