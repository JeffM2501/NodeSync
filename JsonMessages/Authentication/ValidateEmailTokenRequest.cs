using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonMessages.Authentication
{
	public class ValidateEmailTokenRequest : SessionSecuredRequest
	{
		public string EmailToken = string.Empty;
	}
}
