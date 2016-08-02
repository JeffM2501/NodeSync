using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Lidgren.Network;

using NetworkingMessages;
using NetworkingMessages.Messages;


namespace Connector
{

    public class Client
    {
		NetClient SocketClient;

		private int ConnectionPort = 0;
		public int ConnectedPort
		{
			get { return ConnectionPort; }
		}

		protected object Locker = new object();
		protected bool Connected = false;

		public bool IsConnected
		{
			get { lock(Locker) return Connected; }
		}

		public void Connect(string host, int port, bool useNewThred)
		{
			ConnectionPort = port;
			NetPeerConfiguration config = new NetPeerConfiguration(NetworkingMessages.MessageFactory.ProtocolVersionString);
			config.AutoFlushSendQueue = true;
			SocketClient = new NetClient(config);

			SocketClient.RegisterReceivedCallback(new System.Threading.SendOrPostCallback(CheckMessages));
			SocketClient.Start();
			NetOutgoingMessage hail = SocketClient.CreateMessage(NetworkingMessages.MessageFactory.ProtocolVersionString);
			SocketClient.Connect(host, port, hail);
		}

		public void Shutdown()
		{
			if(SocketClient != null)
			{
				SocketClient.Disconnect("Closing");
				SocketClient.FlushSendQueue();

			}
			SocketClient = null;
		}

		private void CheckMessages(object peer)
		{
			while(ProcessOneMessages()) ;
		}

		public event EventHandler HostConnected = null;
		public event EventHandler HostDisconnected = null;

		protected List<NetworkMessage> PendingInboundMessages = new List<NetworkMessage>();

		protected NetworkMessage PopMessage()
		{
			lock(PendingInboundMessages)
			{
				if(PendingInboundMessages.Count == 0)
					return null;

				NetworkMessage msg = PendingInboundMessages[0];
				PendingInboundMessages.RemoveAt(0);
				return msg;
			}
		}

		public void SendMessage(NetworkingMessages.Messages.NetworkMessage msg)
		{
			SendMessage(msg, NetDeliveryMethod.ReliableOrdered, 1);
		}

		public void SendMessage(NetworkingMessages.Messages.NetworkMessage msg, NetDeliveryMethod method, int channel)
		{
			if(SocketClient == null)
				return;
			SocketClient.SendMessage(NetworkingMessages.MessageFactory.PackMessage(SocketClient.CreateMessage(), msg), method, channel);
		}


		private object ExitLocker = new object();
		private bool ExitFlag = false;

		protected bool ExitCheckThread()
		{
			lock(ExitLocker)
				return ExitFlag;
		}

		protected void TerminateCheckThread()
		{
			lock(ExitLocker)
				ExitFlag = true;
		}

		private void CheckMessagesForThread()
		{
			while(!ExitCheckThread())
			{
				while(ProcessOneMessages()) ;
				Thread.Sleep(10);
			}

			CheckingThread = null;
		}

		public bool ProcessOneMessages()
		{
			NetIncomingMessage im;
			if(SocketClient != null && (im = SocketClient.ReadMessage()) != null)
			{
				switch(im.MessageType)
				{
					case NetIncomingMessageType.DebugMessage:
					case NetIncomingMessageType.ErrorMessage:
					case NetIncomingMessageType.WarningMessage:
					case NetIncomingMessageType.VerboseDebugMessage:
						/*AddLogLine(im.ReadString());*/
						break;

					case NetIncomingMessageType.StatusChanged:
						NetConnectionStatus status = (NetConnectionStatus)im.ReadByte();

						string reason = im.ReadString();
						//	AddLogLine(NetUtility.ToHexString(im.SenderConnection.RemoteUniqueIdentifier) + " " + status + ": " + reason);

						if(status == NetConnectionStatus.Connected)
						{
							lock(Locker)
								Connected = true;

							SetClientConnecting();
							// 							if(im.SenderConnection.RemoteHailMessage != null)
							// 								AddLogLine("Remote hail: " + im.SenderConnection.RemoteHailMessage.ReadString());
							if(HostConnected != null)
								HostConnected.Invoke(this, EventArgs.Empty);
						}
						else if(status == NetConnectionStatus.Disconnected)
						{
							lock(Locker)
								Connected = false;

							SetClientDisconnecting();
							if(HostDisconnected != null)
								HostDisconnected.Invoke(this, EventArgs.Empty);
						}
						break;
					case NetIncomingMessageType.Data:
						{
							NetworkMessage msg = MessageFactory.ParseMessage(im);
							lock(PendingInboundMessages)
								PendingInboundMessages.Add(msg);
						}
						break;
					default:
						//AddLogLine("Unhandled type: " + im.MessageType + " " + im.LengthBytes + " bytes " + im.DeliveryMethod + "|" + im.SequenceChannel);
						break;
				}
				if(SocketClient != null)
					SocketClient.Recycle(im);

				return true;
			}
			return false;
		}
	}
}

