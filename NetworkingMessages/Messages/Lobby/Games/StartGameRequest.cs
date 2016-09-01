using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkingMessages.Messages.Lobby.Games
{
    public class StartGameRequest : SerializedNetworkMessage
    {
        public string Name = string.Empty;
        public string Description = string.Empty;
        public string Map = string.Empty;
    }
}
