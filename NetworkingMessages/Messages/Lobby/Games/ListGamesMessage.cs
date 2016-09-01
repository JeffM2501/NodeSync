using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;

namespace NetworkingMessages.Messages.Lobby.Games
{
    public class ListGamesMessage : CustomPackedNetworkMessage
    {
        public class GameInfo
        {
            public string ID = string.Empty;
            public string Name = string.Empty;
            public string Description = string.Empty;
            public string Map = string.Empty;
            public int Players = 0;
        }
        public List<GameInfo> Games = new List<GameInfo>();

        public override NetOutgoingMessage Pack(NetOutgoingMessage msg)
        {
            msg.Write((Int16)Games.Count);
            foreach (var game in Games)
            {
                msg.Write(game.ID);
                msg.Write(game.Name);
                msg.Write(game.Description);
                msg.Write(game.Map);
                msg.Write(game.Players);
            }
            return msg;
        }

        public override void Unpack(NetIncomingMessage msg)
        {
            Games.Clear();
            Int16 count = msg.ReadInt16();
            for (int i = 0; i < count; i++)
            {
                GameInfo u = new GameInfo();
                u.ID = msg.ReadString();
                u.Name = msg.ReadString();
                u.Description = msg.ReadString();
                u.Map = msg.ReadString();
                u.Players = msg.ReadInt32();
                Games.Add(u);
            }
        }
    }
}
