using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonMessages.Authentication
{
	public class ValidateAuthenticationTokenRequest : JsonMessage
	{
		public string UserID = string.Empty;
		public string Token = string.Empty;

		public string APIKey = string.Empty;
	}
}
