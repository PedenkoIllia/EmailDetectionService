using EmailDetectionService.Models;
using MimeKit;
using System;

namespace EmailDetectionService.Helper
{
    public class MessageParser
    {
        public static MessageModel GetMessageModelFromFile(string emailFile)
        {
            var messageHeaders = HeaderList.Load(emailFile);
            MessageModel message = new MessageModel()
            {
                Message_ID = messageHeaders[HeaderId.MessageId],
                Subject = messageHeaders[HeaderId.Subject],
                From = messageHeaders[HeaderId.From],
                To = messageHeaders[HeaderId.To],
                Date = messageHeaders[HeaderId.Date]
            };

            if (message.From == null || message.Date == null)
                throw new FormatException();

            if (String.IsNullOrEmpty(message.Message_ID))
                message.Message_ID = "<" + Guid.NewGuid().ToString() + message.From.Substring(message.From.IndexOf('@')) + ">";

            return message;
        }
    }
}
