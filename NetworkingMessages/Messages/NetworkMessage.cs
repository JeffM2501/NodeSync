using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Lidgren.Network;

namespace NetworkingMessages.Messages
{
    public class NetworkMessage
    {
        public static readonly NetworkMessage Empty = new NetworkMessage();

        public virtual bool CustomPack() { return false; }
        public virtual NetOutgoingMessage Pack(NetOutgoingMessage msg) { return null;}
        public virtual void Unpack(NetIncomingMessage msg) {}
    }

    public class SerializedNetworkMessage : NetworkMessage
    {
        public override bool CustomPack() { return false; }
    }

    public class CustomPackedNetworkMessage : NetworkMessage
    {
        public override bool CustomPack() { return true; }
    }
}
