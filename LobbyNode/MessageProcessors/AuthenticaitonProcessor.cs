using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkingMessages.Messages;

using LobbyNode.Security;

namespace LobbyNode.MessageProcessors
{
	public class AuthenticaitonProcessor : ThreadedMessageProcessor
	{
		LobbyHost Host = null;

        public BanProcessor Bans = null;

        public AuthenticaitonProcessor(LobbyHost host, BanProcessor bans)
		{
            Host = host;
            Bans = bans;

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

            user.SetAttributeB("ValidConnection", false);
            user.SetAttributeB("ValidAuthentication", false);

			// start ban checks
		}

		protected override void HandleUserMessage(LobbyUser user, NetworkMessage message)
		{
			base.HandleUserMessage(user, message);

			// process auth messages
		}

        protected override void Tick()
        {
            base.Tick();

            // check for deaded connections
        }
    }
}
