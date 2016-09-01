using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using Lidgren.Network;

using NetworkingMessages.Messages;

namespace NetworkingMessages
{
	public static class MessageFactory
	{
		public static string ProtocolVersionString = "NetSync.0.0.1";

        private class MessageTypeInfo
        {
            public Type ClassType = null;
            public bool IsCustomPacked = false;
        }

		private static Dictionary<int, MessageTypeInfo> MessageIDs = new Dictionary<int, MessageTypeInfo>();

		static MessageFactory()
		{
			LoadAssembly(Assembly.GetExecutingAssembly());
		}

		public static void LoadAssembly(Assembly assembly)
		{
			Type netMsgType = typeof(NetworkMessage);

			foreach (var t in assembly.GetTypes())
			{
				if (t.IsSubclassOf(netMsgType))
				{
					int id = t.GetHashCode();
					if (MessageIDs.ContainsKey(id))
					{
						MessageIDs.Remove(id);
					}

                    try
                    {
                        MessageTypeInfo info = new MessageTypeInfo();
                        info.ClassType = t;
                        NetworkMessage msg = Activator.CreateInstance(t) as NetworkMessage;
                        info.IsCustomPacked = msg.CustomPack();

                        MessageIDs.Add(id, info);
                    }
                    catch (Exception /*ex*/) { }
        
				}
			}
		}

		public static NetworkMessage ParseMessage(NetIncomingMessage msg)
		{
			if(msg.LengthBytes < sizeof(int))
				return NetworkMessage.Empty;

			int id = msg.ReadInt32();

            MessageTypeInfo t = null;
			lock(MessageIDs)
			{
				if(MessageIDs.ContainsKey(id))
					t = MessageIDs[id];
			}

			if(t == null)
				return NetworkMessage.Empty;

            NetworkMessage outMsg = null;
            outMsg = (NetworkMessage)Activator.CreateInstance(t.ClassType);
            if (t.IsCustomPacked)
                outMsg.Unpack(msg);
            else
                msg.ReadAllFields(outMsg);

			return outMsg;
		}

		public static NetOutgoingMessage PackMessage(NetOutgoingMessage outMsg, NetworkMessage msg)
		{
            outMsg.Write(msg.GetType().GetHashCode());
            if (msg.CustomPack())
                outMsg = msg.Pack(outMsg);
            else
                outMsg.WriteAllFields(msg);

            return outMsg;
		}
	}
}
