using EmailDetectionService.Models;
using System.Text;

namespace EmailDetectionService.Extensions
{
    public static class MessageModelExtension
    {
        public static string ToFormatString(this MessageModel msg)
        {
            StringBuilder sb = new StringBuilder();
            if (msg != null)
            {
                if (msg.Message_ID == null)
                    sb.Append(string.Empty.PadRight(28));
                else if (msg.Message_ID.Length <= 28)
                    sb.Append(msg.Message_ID.PadRight(28));
                else
                    sb.Append(msg.Message_ID.Substring(0, 13)).Append("...").Append(msg.Message_ID.Substring(msg.Message_ID.Length - 12));
                sb.Append('|');

                if (msg.Subject == null)
                    sb.Append(string.Empty.PadRight(35));
                else if (msg.Subject.Length <= 35)
                    sb.Append(msg.Subject.PadRight(35));
                else
                    sb.Append(msg.Subject.Substring(0, 32)).Append("...");
                sb.Append('|');

                if (msg.From == null)
                    sb.Append(string.Empty.PadRight(28));
                else if (msg.From.Length <= 28)
                    sb.Append(msg.From.PadRight(28));
                else
                    sb.Append(msg.From.Substring(0, 25)).Append("...");
                sb.Append('|');

                if (msg.To == null)
                    sb.Append(string.Empty.PadRight(29));
                else if (msg.To.Length <= 28)
                    sb.Append(msg.To.PadRight(28));
                else
                    sb.Append(msg.To.Substring(0, 25)).Append("...");
                sb.Append('|');

                if (msg.Date == null)
                    sb.Append(string.Empty.PadRight(31));
                    sb.Append(msg.Date.PadRight(31));
                sb.Append('|');
            }

            return sb.ToString();
        }
    }
}
