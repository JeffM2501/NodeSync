using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonMessages.Authentication
{
	public class SessionSecuredResponce : JsonMessage
	{
		public string SessionID = string.Empty;
	}
}
