using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Reflection;
using System.Threading;

using WebListener;
using JsonMessages;
using JsonMessages.MessageTypes;
using JsonMessages.MessageTypes.NodeStatus;

namespace NodeController
{
	class Program
	{
        static List<RunningNode> Nodes = new List<RunningNode>();

        static JsonMessageHost InboundControlListener;

        private static string ConfigPath = string.Empty;

        private static ControllerConfig ConfigCache = null;

        static void Main(string[] args)
		{
            if (args.Length > 0)
                ConfigPath = args[0];
            else
                ConfigPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "NodeController.cfg.xml");

            // load config

            var cfg = ControllerConfig.ReadConfig(ConfigPath);

            // setup control listener
            InboundControlListener = new JsonMessageHost();
            InboundControlListener.MessageProcessor = HandleControllMessage;
            InboundControlListener.Prefixes.AddRange(cfg.ListenPrefixes.ToArray());

            InboundControlListener.Startup();

            while (!Done())
            {
                lock(Nodes)
                {
                    foreach(var node in Nodes)
                    {
                        if (node.Alive)
                        {

                        }
                        else
                        {
                            // notify them that the node died
                        }
                    }
                }
                Thread.Sleep(100);
            }

            InboundControlListener.Shutdown();
        }

        static ControllerConfig GetConfig()
        {
            if (ConfigCache == null)
                ConfigCache = ControllerConfig.ReadConfig(ConfigPath);
            return ConfigCache;
        }

        static JsonMessage HandleControllMessage(JsonMessage request, JsonMessageHost.SessionManager sessions)
        {
            if (!ValidateTokenRequest(request as TokenAuthenticatedRequest))
                return null;

            TokenAuthenticatedResponce responce = null;

            if (request as NodeStatusRequest != null)
                responce = BuildNodeStatus();

            return SignResponce(responce, request as TokenAuthenticatedRequest);
        }

        static TokenAuthenticatedResponce SignResponce(TokenAuthenticatedResponce responce, TokenAuthenticatedRequest request)
        {
            if (responce != null && request != null)
                responce.Token = EncodingTools.Encryption.Encrypt(request.Token, GetConfig().GetCrypto(request.HostID));
            
            return responce;
        }

        static bool ValidateTokenRequest(TokenAuthenticatedRequest request)
        {
            if (request == null)
                return false;

            var cfg = GetConfig();
            var hostInfo = cfg.FindHost(request.HostID);
            if (hostInfo == null)
                return false;
            return EncodingTools.Tokens.ValidateCurrentTimeToken(cfg.GetCrypto(hostInfo.Name), request.Token, cfg.TokenKeyValidationRange);
        }

        private static NodeStatusResponce BuildNodeStatus()
        {
            NodeStatusResponce msg = new NodeStatusResponce();
            lock (Nodes)
            {
                foreach(RunningNode node in Nodes)
                {
                    if (node.Alive)
                        msg.ActiveNodes.Add(node.Info);
                }
            }

            return msg;
        }

        private static bool Done()
        {
            return false;
        }

        public class RunningNode : NodeIOLink
        {
            private static Random RNG = new Random();

            public static string NodeExeLocation = string.Empty;

            public DirectoryInfo NodeTempDir = null;

            public NodeIOLink Link = new NodeIOLink();

            public NodeStatusResponce.NodeInfo Info = new NodeStatusResponce.NodeInfo();

            public RunningNode()
            {
                Link.MessageReceived += Link_MessageReceived;

                NodeTempDir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), RNG.Next().ToString() + RNG.Next().ToString()));
                try
                {
                    NodeTempDir.Create();
                }
                catch (Exception /*ex*/)
                {
                }
            }

            private void Link_MessageReceived(object sender, EventArgs e)
            {
               
            }

            public void Startup()
            {
                Link.Startup(NodeExeLocation);
            }

            public void Process()
            {

            }
        }
	}
}
