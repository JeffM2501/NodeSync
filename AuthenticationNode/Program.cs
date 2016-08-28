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

			SetupAPI(cfg.PlugInsFolder);

			Server s = new Server(cfg);

			s.Startup();

			while(!s.TimeToQuit())
				Thread.Sleep(100);

			s.Shutdown();
		}

		static void SetupAPI(string path)
		{

			if (Directory.Exists(path))
			{
				foreach(var f in new DirectoryInfo(path).GetFiles("*.dll"))
				{
					try
					{
						APIManager.InitPluginsInAssembly(Assembly.LoadFile(f.FullName));
					}
					catch (System.Exception /*ex*/)
					{
						
					}
				}
			}
		}
	}
}
