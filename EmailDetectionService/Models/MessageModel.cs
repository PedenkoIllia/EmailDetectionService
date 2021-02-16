using System;

namespace EmailDetectionService.Models
{
    public class MessageModel
    {
        public string Message_ID { get; set; }
        public string Subject { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public DateTime Date { get; set; }

        public override string ToString()
        {
            return $"{Message_ID} | {Subject} | {From ?? "*None*"} | {To ?? "*None*"} | {Date.ToString()}";
        }
    }
}
