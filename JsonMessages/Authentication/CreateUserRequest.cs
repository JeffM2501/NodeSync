using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonMessages.Authentication
{
	public class CreateUserRequest : JsonMessage
	{
		public string Email = string.Empty;
		public string Password = string.Empty;
	}
}
