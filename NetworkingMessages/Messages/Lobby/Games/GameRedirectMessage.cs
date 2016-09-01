using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkingMessages.Messages.Lobby.Games
{
    public class GameRedirectMessage : SerializedNetworkMessage
    {
        public bool Optional = false;
        public string Message = string.Empty;

        public string ID = string.Empty;
        public string Host = string.Empty;
        public int Port = int.MinValue;

        public string JoinTicket = string.Empty;
    }
}
