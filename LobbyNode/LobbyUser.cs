using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Listener;

using NetworkingMessages.Messages;

namespace LobbyNode
{
	public interface IUserMessageProcessor
	{
		void PeerAdded(LobbyUser user);

		void PeerRemoved(LobbyUser user);

		void ReceivePeerData(NetworkMessage msg, LobbyUser user);
	}

	public class LobbyUser : Peer
	{
		public IUserMessageProcessor MessageProcessor = null;

		public string GlobalUserID = string.Empty;
		public string GlobalToken = string.Empty;

		public enum AuthenticationStatusTypes
		{
			Unknown,
			Connected,
			Pending,
			Valid,
			Invalid,
		}
		public AuthenticationStatusTypes AuthenticationStatus = AuthenticationStatusTypes.Unknown;


		private List<NetworkMessage> PendingInboundMessages = new List<NetworkMessage>();

		public void PushInboundMessage(NetworkMessage msg)
		{
			lock(PendingInboundMessages)
				PendingInboundMessages.Add(msg);
		}

		public NetworkMessage PopInboundMessage()
		{
			NetworkMessage msg = null;
			lock(PendingInboundMessages)
			{
				if (PendingInboundMessages.Count > 0)
				{
					msg = PendingInboundMessages[0];
					PendingInboundMessages.RemoveAt(0);
				}
			}
			return msg;
		}
	}
}
