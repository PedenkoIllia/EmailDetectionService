using System;
using Xunit;

namespace EmailDetectionServiceTetsts
{
    public class EmailProcessorTest
    {
        [Theory]
        [InlineData(@".\")]
        [InlineData(@"")]
        public void ParseMessageFromFileTest_Normal(string emailFileName)
        {

        }
    }
}
