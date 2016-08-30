using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkingMessages.Messages.Lobby
{
	public class AuthenticationRequest : NetworkMessage
	{
		public string UserID = string.Empty;
		public string Token = string.Empty;

		public string DisplayName = string.Empty;
	}
}
