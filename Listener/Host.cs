using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Lidgren.Network;

namespace Listener
{
	public class Host
	{
		private NetServer SocketServer = null;
		protected int ActualPort = -1;

		public void Listen(int port)
		{
			if(port == 0)
				port = new Random().Next(1024, Int16.MaxValue - 10);
			NetPeerConfiguration config = new NetPeerConfiguration(NetworkingMessages.MessageFactory.ProtocolVersionString);

			config.AutoFlushSendQueue = true;
			config.MaximumConnections = 200;
			config.ConnectionTimeout = 10;
#if(DEBUG)
			config.ConnectionTimeout = 100;
#endif
			config.Port = port;
			SocketServer = new NetServer(config);
			SocketServer.Start();

			ActualPort = port;
		}

		public virtual void Shutdown()
		{
			SocketServer.Shutdown("Closing");
			SocketServer = null;
		}

		public event EventHandler LogLineAdded = null;

		protected List<string> LogLines = new List<string>();
		public string GetLogLine()
		{
			lock(LogLines)
			{
				if(LogLines.Count == 0)
					return string.Empty;

				string l = LogLines[0];
				LogLines.RemoveAt(0);
				return l;
			}
		}

		protected void AddLogLine(string text)
		{
			lock(LogLines)
				LogLines.Add(text);

			LogLineAdded?.Invoke(this, EventArgs.Empty);
		}

		protected object Locker = new object();
		protected bool Connected = false;

		public bool IsConnected
		{
			get { lock(Locker) return Connected; }
		}

		protected Dictionary<long, Peer> ConnectedPeers = new Dictionary<long, Peer>();

		public PeerHandler DefaultPeerHandler = null;

		protected NetIncomingMessage PollMessage()
		{
			if(SocketServer == null)
				return null;

			lock(SocketServer)
			{
				return SocketServer.ReadMessage();
			}
		}

		protected bool SocketIsLive()
		{
			if(SocketServer == null)
				return false;

			lock(SocketServer)
			{
				return SocketServer.Status == NetPeerStatus.Running || SocketServer.Status == NetPeerStatus.Starting;
			}
		}

		public void ProcessSockets()
		{
			Peer peer = null;
			NetIncomingMessage im;
			while((im = PollMessage()) != null)
			{
				long id = long.MinValue;
				if(im != null && im.SenderConnection != null)
					id = im.SenderConnection.RemoteUniqueIdentifier;

				peer = null;

				switch(im.MessageType)
				{
					case NetIncomingMessageType.DebugMessage:
					case NetIncomingMessageType.ErrorMessage:
					case NetIncomingMessageType.WarningMessage:
					case NetIncomingMessageType.VerboseDebugMessage:
						AddLogLine(im.ReadString());
						break;

					case NetIncomingMessageType.StatusChanged:
						NetConnectionStatus status = (NetConnectionStatus)im.ReadByte();

						string reason = im.ReadString();
						AddLogLine(NetUtility.ToHexString(im.SenderConnection.RemoteUniqueIdentifier) + " " + status + ": " + reason);

						if(status == NetConnectionStatus.Connected)
						{
							lock(Locker)
								Connected = true;

							AddLogLine("Remote hail: " + im.SenderConnection.RemoteHailMessage.ReadString());

					
							if(DefaultPeerHandler == null)
								im.SenderConnection.Disconnect("Denied");
							else
							{
								peer = DefaultPeerHandler.AddPeer(im);

								lock(ConnectedPeers)
									ConnectedPeers.Add(im.SenderConnection.RemoteUniqueIdentifier, peer);
							}

						}
						else if(status == NetConnectionStatus.Disconnected)
						{
							peer = ConnectedPeers.ContainsKey(id) ? ConnectedPeers[id] : null;

							if(peer == null)   // not one of ours
								return;

							// tell our handler we poofed
							if(peer.Handler != null)
								peer.Handler.PeerDisconnected(reason, peer);

							lock(ConnectedPeers)
							{
								Connected = ConnectedPeers.Count > 0;
								ConnectedPeers.Remove(im.SenderConnection.RemoteUniqueIdentifier);
							}
						}
						break;
					case NetIncomingMessageType.Data:

						peer = ConnectedPeers.ContainsKey(id) ? ConnectedPeers[id] : null;

						if(peer == null || peer.Handler == null)   // not one of ours
							return;

						// tell our handler we got some data
						peer.Handler.PeerReceiveData(im, peer);

						break;

					default:
						AddLogLine("Unhandled type: " + im.MessageType + " " + im.LengthBytes + " bytes " + im.DeliveryMethod + "|" + im.SequenceChannel);
						break;
				}
				SocketServer.Recycle(im);
			}
		}
	}
}
