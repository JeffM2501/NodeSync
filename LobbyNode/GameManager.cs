using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using WebConnector;

using JsonMessages;
using JsonMessages.MessageTypes.NodeStatus;
using JsonMessages.MessageTypes;

namespace LobbyNode
{
    public static class GameManager
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
        }

        public class NodeControllerInfo
        {
            public string ID = string.Empty;
            public string Name = string.Empty;

            public string NodeHost = string.Empty;
            public string ManagementURL = string.Empty;

            public string APIKey = string.Empty;

            public RijndaelManaged CryptoCache = new RijndaelManaged();;

            public double Utilization = 0.0;
            public int MaxGames = 10;

            public List<GameInfo> HostedGames = new List<GameInfo>();

            public DateTime LastHeartBeat = DateTime.MinValue;

            public JsonClient Connection = null;

            public event EventHandler HostedGamesListUpdated = null;

            public void Setup()
            {
                CryptoCache = EncodingTools.Encryption.BuildCrypto(APIKey);

                Connection = new JsonClient(ManagementURL);
                Connection.ReceivedResponce += Connection_ReceivedResponce;
                UpdateGameList();
            }

            private void Connection_ReceivedResponce(object sender, JsonClient.JsonMessageResponceArgs e)
            {
                if (!ValidateResponce(e.RequestMessage as TokenAuthenticatedRequest, e.ResponceMessage as TokenAuthenticatedResponce))
                    return;

                if (e.ResponceMessage as NodeStatusResponce != null)
                    HandleNodeStatusResponce(e.ResponceMessage as NodeStatusResponce);
            }

            protected void HandleNodeStatusResponce(NodeStatusResponce responce)
            {
                if (responce == null)
                    return;

                Utilization = responce.Utilization;

                lock (HostedGames)
                {
                    HostedGames.Clear();

                    foreach (var g in responce.ActiveNodes)
                    {
                        GameInfo info = new GameInfo();
                        info.ID = g.ID;
                        info.Name = g.Name;
                        info.NodeController = this;
                        info.Map = g.Map;
                        info.Host = NodeHost;
                        info.Port = g.Port;
                        info.Description = g.Description;

                        HostedGames.Add(info);
                    }
                }

                HostedGamesListUpdated?.Invoke(this, EventArgs.Empty);
            }

            public void UpdateGameList()
            {
                SendRequest(new NodeStatusRequest(), null);
            }

            public TokenAuthenticatedRequest SignSecureRequest(TokenAuthenticatedRequest request)
            {
                request.HostID = ID;
                request.Token = EncodingTools.Tokens.GenerateTimeToken(CryptoCache);
                return request;
            }

            public bool ValidateResponce(TokenAuthenticatedRequest request, TokenAuthenticatedResponce responce)
            {
                if (request == null || responce == null)
                    return true;    // unsecured messages are valid

                string decodedToken = EncodingTools.Encryption.Decrypt(responce.Token, CryptoCache);
                return decodedToken == request.Token;
            }

            void SendRequest(JsonMessage msg, object token)
            {
                if (msg as TokenAuthenticatedRequest != null)
                    msg = SignSecureRequest(msg as TokenAuthenticatedRequest);

                Connection.SendMessage(msg, token);
            }
        }

        public static List<NodeControllerInfo> NodeControllers = new List<NodeControllerInfo>();

        public static void Setup(LobbyHost.Config config)
        {
            foreach(var c in config.NodeControllers)
            {
                NodeControllerInfo info = new NodeControllerInfo();
                info.ID = c.HostIdentifyer;
                info.Name = c.Name;
                info.APIKey = c.APIKey;
                info.APIKey = c.APIKey;
                info.ManagementURL = c.ControlConnectionURL;

                info.HostedGamesListUpdated += Info_HostedGamesListUpdated;
                info.Setup();

                NodeControllers.Add(info);
            }
        }

        private static void Info_HostedGamesListUpdated(object sender, EventArgs e)
        {
            
        }
    }
}
