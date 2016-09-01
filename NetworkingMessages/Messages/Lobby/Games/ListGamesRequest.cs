using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkingMessages.Messages.Lobby.Games
{
    public class ListGamesRequest : SerializedNetworkMessage
    {
        public string Filter = string.Empty;
    }
}
