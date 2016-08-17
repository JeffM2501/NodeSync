using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;

using WebListener;
using JsonMessages;

namespace NodeController
{
	class Program
	{
        static List<RunningNode> Nodes = new List<RunningNode>();

        static JsonMessageHost InboundControlListener;

        private static string ConfigPath = string.Empty;

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
                Thread.Sleep(100);
            }

            InboundControlListener.Shutdown();
        }

        static ControllerConfig GetConfig()
        {
            return ControllerConfig.ReadConfig(ConfigPath);
        }

        static JsonMessage HandleControllMessage(JsonMessage request, JsonMessageHost.SessionManager sessions)
        {
            var cfg = GetConfig();

            return null;
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
