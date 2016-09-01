using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lidgren.Network;

namespace NetworkingMessages.Messages.Lobby
{
    public class AddLobbyChatUsers: CustomPackedNetworkMessage
    {
        public class LobbChatyUser
        {
            public string UserID = string.Empty;
            public string DisplayName = string.Empty;
        }
        public List<LobbChatyUser> ChatUsers = new List<LobbChatyUser>();

        public override NetOutgoingMessage Pack(NetOutgoingMessage msg)
        {
            msg.Write((Int16)ChatUsers.Count);
            foreach (var user in ChatUsers)
            { 
                msg.Write(user.UserID);
                msg.Write(user.DisplayName);
            }
            return msg;
        }

        public override void Unpack(NetIncomingMessage msg)
        {
            ChatUsers.Clear();
            Int16 count = msg.ReadInt16();
            for(int i = 0; i < count; i++)
            {
                LobbChatyUser u = new LobbChatyUser();
                u.UserID = msg.ReadString();
                u.DisplayName = msg.ReadString();
                ChatUsers.Add(u);
            }
        }
    }
}
