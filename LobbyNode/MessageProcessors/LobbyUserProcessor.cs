using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkingMessages.Messages;

namespace LobbyNode.MessageProcessors
{
    public class LobbyUserProcessor : ThreadedMessageProcessor
    {
        protected override void HandleUserAdded(LobbyUser user)
        {
            base.HandleUserAdded(user);
        }

        protected override void HandleUserRemoved(LobbyUser user)
        {
            base.HandleUserRemoved(user);
        }

        protected override void HandleUserMessage(LobbyUser user, NetworkMessage message)
        {
            base.HandleUserMessage(user, message);
        }
    }
}
