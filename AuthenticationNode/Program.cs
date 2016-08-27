using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Threading;

namespace AuthenticationNode
{
	class Program
	{
		static void Main(string[] args)
		{
			string configPath = string.Empty;
			if(args.Length > 0)
				configPath = args[0];

			if (configPath == string.Empty)
				configPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "auth.config.xml");

			AuthConfig cfg = AuthConfig.Load(configPath);

			Server s = new Server(cfg);

			s.Startup();

			while(true)
				Thread.Sleep(100);

			s.Shutdown();
		}
	}
}
