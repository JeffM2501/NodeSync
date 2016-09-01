using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;

using Lidgren.Network;

using Listener;

using WebConnector;

using JsonMessages.MessageTypes.NodeStatus;
using JsonMessages.Authentication;

using NetworkingMessages;
using NetworkingMessages.Messages;
using LobbyNode.MessageProcessors;


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

			public class AuthenticationNodeInfo
			{
				public string Name = string.Empty;
				public string APIKey = string.Empty;
				public string Host = string.Empty;

                internal JsonClient WebConnection = null;
                internal RijndaelManaged Crypto = null;

                public void Setup()
                {
                    WebConnection = new JsonClient(Host);
                    Crypto = EncodingTools.Encryption.BuildCrypto(APIKey);
                }

                public void StartValidationRequest(string userID, string token, LobbyUser user, EventHandler<JsonClient.JsonMessageResponceArgs> callback)
                {
                    ValidateAuthenticationTokenRequest request = new ValidateAuthenticationTokenRequest();
                    request.UserID = userID;
                    request.Token = token;
                    request.APIKey = string.Empty;

                    WebConnection.SendMessage(request, user, callback);
                }
			}
            public List<AuthenticationNodeInfo> AuthenticationEndpoints = new List<AuthenticationNodeInfo>();

			public int AutenticationProcessorThreads = 1;

            public int MaxConnections = 200;

			internal List<NodeControllerLinkConfig> NodeControllers = new List<NodeControllerLinkConfig>();

            public string BanListPath = string.Empty;
        }

		public Config LobbyConfig = null;

        protected Random RNG = new Random();

		public class NodeControllerLink
		{
			public JsonClient ControlLink = null;

			public NodeControllerLinkConfig Config = new NodeControllerLinkConfig();

			public NodeStatusResponce LastStatusUpdate = null;
		}

		public List<NodeControllerLink> NodeControllerLinks = new List<NodeControllerLink>();

        protected Security.BanProcessor BanManager = new Security.BanProcessor();

		protected List<AuthenticaitonProcessor> AuthenticationPool = new List<AuthenticaitonProcessor>();
		protected int LastAuthenticator = 0;

		public LobbyHost(string nodeConfigPath) : base()
		{
			DefaultPeerHandler = this;
            LoadConfigs(nodeConfigPath);

			SetupProcessingPools();
		}

		public override void Shutdown()
		{
			foreach(var p in AuthenticationPool)
				p.Stop();
			
			base.Shutdown();
		}

		protected void SetupProcessingPools()
        {
            if (LobbyConfig.BanListPath != string.Empty)
                BanManager.Bans = Security.BanList.LoadFromXML(LobbyConfig.BanListPath);
            else
                BanManager.Bans = new Security.BanList();

            foreach (var auth in LobbyConfig.AuthenticationEndpoints)
                auth.Setup();

            for (int i = 0; i < LobbyConfig.AutenticationProcessorThreads; i++)
				AuthenticationPool.Add(new AuthenticaitonProcessor(this, BanManager));
		}

		protected AuthenticaitonProcessor GetNextAuthProcessor()
		{
			if(LobbyConfig.AutenticationProcessorThreads != 1)
			{
				LastAuthenticator++;
				if(LastAuthenticator >= LobbyConfig.AutenticationProcessorThreads)
					LastAuthenticator = 0;
			}
			return AuthenticationPool[LastAuthenticator];
		}

		public static string MainConfigName = "lobby_conf.xml";

		protected void LoadConfigs(string nodeConfigPath)
		{
			LobbyConfig = null;
			DirectoryInfo dir = new DirectoryInfo(nodeConfigPath);
            XmlSerializer xml = new XmlSerializer(typeof(Config));
			try
			{
				FileInfo file = new FileInfo(Path.Combine(dir.FullName, MainConfigName));
				StreamReader sr = file.OpenText();
				LobbyConfig = xml.Deserialize(sr) as Config;
				sr.Close();
				if(LobbyConfig != null)
				{
					xml = new XmlSerializer(typeof(NodeControllerLinkConfig));
					foreach(FileInfo nodeConfig in dir.GetFiles("*.node.xml"))
					{
						sr = nodeConfig.OpenText();
						NodeControllerLinkConfig node = xml.Deserialize(sr) as NodeControllerLinkConfig;
						sr.Close();
						if (node != null)
                        {
                            LobbyConfig.NodeControllers.Add(node);
                        }
							
					}
				}
			}
			catch (System.Exception /*ex*/)
			{
				
			}

			if (LobbyConfig == null)
				LobbyConfig = new Config();
		}

		Peer PeerHandler.AddPeer(NetIncomingMessage msg)
		{
			LobbyUser user = new LobbyUser();
			user.MessageProcessor = GetNextAuthProcessor();
			if (user.MessageProcessor != null)
				user.MessageProcessor.PeerAdded(user);
			return user;
		}

		void PeerHandler.DisconnectPeer(string reason, Peer peer)
		{
			peer.SocketConnection.Disconnect(reason);
		}

		void PeerHandler.PeerDisconnected(string reason, Peer peer)
		{
			LobbyUser user = peer as LobbyUser;
			if(user != null && user.MessageProcessor != null)
				user.MessageProcessor.PeerRemoved(user);
		}

		void PeerHandler.PeerReceiveData(NetIncomingMessage msg, Peer peer)
		{
			LobbyUser user = peer as LobbyUser;
			if(user == null)
				return;

			if(user.MessageProcessor != null)
				user.MessageProcessor.ReceivePeerData(MessageFactory.ParseMessage(msg), user);
		}

        public void SendValidationRequest(string userID, string token, LobbyUser user, EventHandler<JsonClient.JsonMessageResponceArgs> callback)
        {
      
            Config.AuthenticationNodeInfo authNode = null;

            lock (LobbyConfig.AuthenticationEndpoints)
            {
                if (LobbyConfig.AuthenticationEndpoints.Count == 0)
                    return;

                if (LobbyConfig.AuthenticationEndpoints.Count == 1)
                    authNode = LobbyConfig.AuthenticationEndpoints[0];
                else
                    authNode = LobbyConfig.AuthenticationEndpoints[RNG.Next(LobbyConfig.AuthenticationEndpoints.Count)];
            }

            if (authNode == null)
                return;

            authNode.StartValidationRequest(userID, token, user, callback);
        }
    }
}
