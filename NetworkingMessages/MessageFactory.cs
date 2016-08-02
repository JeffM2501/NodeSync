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

		private static Dictionary<int, Type> MessageIDs = new Dictionary<int, Type>();

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

					MessageIDs.Add(id, t);
				}
			}
		}

		public static NetworkMessage ParseMessage(NetIncomingMessage msg)
		{
			if(msg.LengthBytes < sizeof(int))
				return NetworkMessage.Empty;

			int id = msg.ReadInt32();

			Type t = null;
			lock(MessageIDs)
			{
				if(MessageIDs.ContainsKey(id))
					t = MessageIDs[id];
			}

			if(t == null)
				return NetworkMessage.Empty;

			NetworkMessage outMsg = (NetworkMessage)Activator.CreateInstance(t);
			msg.ReadAllFields(outMsg);

			return outMsg;
		}

		public static NetOutgoingMessage PackMessage(NetOutgoingMessage outMsg, NetworkMessage msg)
		{
			outMsg.Write(msg.GetType().GetHashCode());
			outMsg.WriteAllFields(msg);
			return outMsg;
		}
	}
}
