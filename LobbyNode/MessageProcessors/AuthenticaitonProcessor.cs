using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkingMessages.Messages;
using NetworkingMessages.Messages.Lobby;

using LobbyNode.Security;
using WebConnector;
using JsonMessages.Authentication;

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

			user.AuthenticationStatus = LobbyUser.AuthenticationStatusTypes.Pending;
			user.SetAttributeB("ValidConnection", false);
            user.SetAttributeB("ValidAuthentication", false);

			// start ban checks
			Bans.Add(user, BanChecksCompleted, null);
		}

		protected override void HandleUserMessage(LobbyUser user, NetworkMessage message)
		{
			base.HandleUserMessage(user, message);

			// process auth messages
			if(message as AuthenticationRequest != null)
				ProcessAuthenticationRequest(user, message as AuthenticationRequest);
		}

        protected override void Tick()
        {
            base.Tick();

            // check for deaded connections
        }

		protected void CheckConnection(LobbyUser user)
		{
			if (user.GetAttributeB("ValidAuthentication") == true && user.GetAttributeB("ValidConnection") == true)
			{
				user.AuthenticationStatus = LobbyUser.AuthenticationStatusTypes.Valid;
				Host.PeerAuthenticated(user);
			}
		}

		protected void ProcessAuthenticationRequest(LobbyUser user, AuthenticationRequest request)
		{
			if(request == null)
				return;

			user.SetAttributeB("ValidAuthentication", false);
			user.GlobalUserID = request.UserID;
			user.GlobalToken = request.Token;
			user.DisplayName = request.DisplayName;

			Host.SendValidationRequest(user.UserID, request.Token, user, ArgChecksCallback);
		}

		protected void BanChecksCompleted(object sender, BanProcessrUserArgs args)
		{
			if(args.Results != BanProcessrUserArgs.ResultTypes.Accepted)
				args.User.SocketConnection.Disconnect(args.Results.ToString());
			else
			{
				args.User.SetAttributeB("ValidConnection", true);
				CheckConnection(args.User);
			}
		}

		protected void ArgChecksCallback(object sender, JsonClient.JsonMessageResponceArgs args)
		{
			LobbyUser user = args.Token as LobbyUser;
			if(user == null)
				return;

			if (args.ResponceMessage as ValidateAuthenticationTokenResponce == null)
				user.SocketConnection.Disconnect("Invalid Authentication");
			else
			{
				user.SetAttributeB("ValidAuthentication", true);
				CheckConnection(user);
			}
		}

	}
}
