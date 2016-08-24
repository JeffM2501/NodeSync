using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using Lidgren.Network;
using Listener;
using WebConnector;
using JsonMessages.MessageTypes.NodeStatus;


namespace LobbyNode
{
	public class LobbyHost : Host, PeerHandler
	{
        public class NodeControllerLinkConfig
        {
            public string HostIdentifyer = string.Empty;
            public string ControlConnectionURL = string.Empty;

            public string OutboundPrivateKey = string.Empty;
            public string InboundAPIKey = string.Empty;

            // add other data here to describe the controller
        }

        public class Config
        {
            public int ListenPort = 22122;

            public List<string> AuthenticationServerURLs = new List<string>();

            public int MaxConnections = 200;
            public List<string> ChatRelayServers = new List<string>();
        }
		
		public class NodeControllerLink
		{
			public JsonClient ControlLink = null;

			public NodeControllerLinkConfig Config = new NodeControllerLinkConfig();

			public NodeStatusResponce LastStatusUpdate = null;
		}

		public List<NodeControllerLink> NodeControllerLinks = new List<NodeControllerLink>();

		public LobbyHost(string nodeConfigPath) : base()
		{
			DefaultPeerHandler = this;
			LoadConfigs(nodeConfigPath);
		}

        public static string MainConfigName = "lobby_conf.xml";

		protected void LoadConfigs(string nodeConfigPath)
		{
            DirectoryInfo dir = new DirectoryInfo(nodeConfigPath);

            XmlSerializer xml = new XmlSerializer(typeof(Config));
		}

		Peer PeerHandler.AddPeer(NetIncomingMessage msg)
		{
			throw new NotImplementedException();
		}

		void PeerHandler.DisconnectPeer(string reason, Peer peer)
		{
			throw new NotImplementedException();
		}

		void PeerHandler.PeerDisconnected(string reason, Peer peer)
		{
			throw new NotImplementedException();
		}

		void PeerHandler.PeerReceiveData(NetIncomingMessage msg, Peer peer)
		{
			throw new NotImplementedException();
		}
	}
}
