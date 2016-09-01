using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkingMessages.Messages.Chat
{
    public class UserChatMessage : SerializedNetworkMessage
    {
        public string FromUserID = string.Empty;
        public string FromDisplayName = string.Empty;
        public string ChatMessage = string.Empty;
    }
}
