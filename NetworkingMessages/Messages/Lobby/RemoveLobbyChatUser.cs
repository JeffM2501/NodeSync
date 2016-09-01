using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkingMessages.Messages.Lobby
{
    public class RemoveLobbyChatUser : SerializedNetworkMessage
    {
        public string UserID = string.Empty;
        public string ReasonMessage = string.Empty;
    }
}
