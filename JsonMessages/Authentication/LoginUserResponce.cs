using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonMessages.Authentication
{
	public class LoginUserResponce : JsonMessage
	{
		public bool OK = false;
		public string UserID = string.Empty;
		public string Responce = string.Empty;
		public string SessionID = string.Empty;
	}
}
