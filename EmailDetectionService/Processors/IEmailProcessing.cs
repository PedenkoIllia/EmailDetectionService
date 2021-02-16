using System.Collections.Generic;

namespace EmailDetectionService.Processors
{
    public interface IEmailProcessing
    {
        void StartProcessMessages();
        void ProcessMessagesQueue(IEnumerable<string> emailFiles);
        void StopProcessMessages();
    }
}
