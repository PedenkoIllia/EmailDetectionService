using EmailDetectionService.Models;
using System.Collections.Generic;

namespace EmailDetectionService.DAL
{
    public interface IDataSourceDetectedMessages
    {
        List<MessageModel> GetMessages(int maxCount = 100);
        bool InsertMessage(MessageModel message);
        bool InsertMessagesScope(List<MessageModel> messages);
    }
}
