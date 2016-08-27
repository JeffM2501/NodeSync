using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonMessages.Authentication
{
	public class LoginUserRequest : JsonMessage
	{
		public string Email;
		public string Password;
	}
}
