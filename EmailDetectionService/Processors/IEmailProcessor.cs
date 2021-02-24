using System.Collections.Generic;

namespace EmailDetectionService.Processors
{
    public interface IEmailProcessor
    {
        void StartProcessMessages();
        void ProcessMessagesQueue(IEnumerable<string> emailFiles);
        void StopProcessMessages();
    }
}
