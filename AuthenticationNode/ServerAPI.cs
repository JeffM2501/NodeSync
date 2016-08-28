using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace AuthenticationNode
{
	public interface IPlugin
	{
		void PluginStartup();
		void PluginShutdown();
	}

	internal static class APIManager
	{
		public static List<IPlugin> Plugins = new List<IPlugin>();

		public static void InitPluginsInAssembly(Assembly ass)
		{
			lock (Plugins)
			{
				foreach(Type t in ass.GetTypes())
				{
					if(t.GetInterface(typeof(IPlugin).Name) != null)
					{
						IPlugin p = Activator.CreateInstance(t) as IPlugin;
						Plugins.Add(p);
						p.PluginStartup();
					}
				}
			}
		}

		public static void ShutdownAllPlugins()
		{
			lock(Plugins)
			{
				foreach(var p in Plugins)
					p.PluginShutdown();
			}
		}
	}

	public interface TokenProcessor
	{
		string GenerateAuthToken(string userID, RijndaelManaged crypto);
		bool ValidateAuthToken(string userID, string token, RijndaelManaged crypto);
	}

	public static class API
	{
		internal static TokenProcessor LastProcessor = null;
		internal static List<TokenProcessor> TokenProcessors = new List<TokenProcessor>();

		public static void RegisterTokenProcessor(TokenProcessor procesor)
		{
			TokenProcessors.Add(procesor);
			LastProcessor = procesor;
		}

		public static void RemoveTokenProcessor(TokenProcessor procesor)
		{
			TokenProcessors.Remove(procesor);
			if(TokenProcessors.Count == 0)
				LastProcessor = null;
			else
				LastProcessor = TokenProcessors[TokenProcessors.Count - 1];
		}
	}
}
