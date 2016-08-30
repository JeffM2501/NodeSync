using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkingMessages.Messages;

namespace LobbyNode.MessageProcessors
{
	public class AuthenticaitonProcessor : ThreadedMessageProcessor
	{
		LobbyHost.Config LobbyConfig = null;

		public AuthenticaitonProcessor(LobbyHost.Config config)
		{
			LobbyConfig = config;
			Start();
		}

		protected override void Startup()
		{
			// setup web connections
			base.Startup();
		}

		protected override void Shutdown()
		{
			base.Shutdown();
		}

		protected override void HandleUserAdded(LobbyUser user)
		{
			base.HandleUserAdded(user);

			// start ban checks
		}

		protected override void HandleUserMessage(LobbyUser user, NetworkMessage message)
		{
			base.HandleUserMessage(user, message);

			// process auth messages
		}
	}
}
