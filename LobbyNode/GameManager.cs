using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WebConnector;

using JsonMessages.MessageTypes.NodeStatus;

namespace LobbyNode
{
    public class GameManager
    {
        public class GameInfo
        {
            public string ID = string.Empty;
            public string Host = string.Empty;
            public int Port = int.MinValue;

            public string Name = string.Empty;
            public string Description = string.Empty;
            public string Map = string.Empty;

            public NodeControllerInfo NodeController = null;

            public int Players = int.MinValue;
            public int LastPing = int.MinValue;

            public bool AcceptingNewConnections = true;

            public class RedirectedPlayer
            {
                public string UserID = string.Empty;
                public string JoinToken = string.Empty;
                public DateTime RedirectTime = DateTime.MinValue;
            }
            public List<RedirectedPlayer> KnownPlayers = new List<RedirectedPlayer>();
        }

        public Dictionary<string, GameInfo> ActiveGames = new Dictionary<string, GameInfo>();

        public class NodeControllerInfo
        {
            public string ID = string.Empty;
            public string Name = string.Empty;

            public string ManagementURL = string.Empty;

            public string OutboundPrivateKey = string.Empty;
            public string InboundAPIKey = string.Empty;

            public double Utilization = 0.0;
            public int MaxGames = 10;

            public List<GameInfo> HostedGames = new List<GameInfo>();

            public DateTime LastHeartBeat = DateTime.MinValue;

            public JsonClient Connection = null;

            public void Setup()
            {
                Connection = new JsonClient(ManagementURL);
                Connection.ReceivedResponce += Connection_ReceivedResponce;
                UpdateGameList();
            }

            private void Connection_ReceivedResponce(object sender, JsonClient.JsonMessageResponceArgs e)
            {
                if (e.ResponceMessage as NodeStatusResponce != null)
                {

                }
            }

            public void UpdateGameList()
            {
                Connection.SendMessage(new NodeStatusRequest(), null);
            }
        }

        public List<NodeControllerInfo> NodeControllers = new List<NodeControllerInfo>();

        public void Setup(LobbyHost.Config config)
        {
            foreach(var c in config.NodeControllers)
            {
                NodeControllerInfo info = new NodeControllerInfo();
                info.ID = c.HostIdentifyer;
                info.Name = c.Name;
                info.OutboundPrivateKey = c.OutboundPrivateKey;
                info.InboundAPIKey = c.InboundAPIKey;
                info.ManagementURL = c.ControlConnectionURL;

                NodeControllers.Add(info);
            }
        }
    }
}
