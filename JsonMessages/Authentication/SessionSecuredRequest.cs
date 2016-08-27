using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonMessages.Authentication
{
	public class SessionSecuredRequest : JsonMessage
	{
		public string UserID = string.Empty;
		public string SessionID = string.Empty;
	}
}
