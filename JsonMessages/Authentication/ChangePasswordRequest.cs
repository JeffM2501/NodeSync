using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonMessages.Authentication
{
	public class ChangePasswordRequest : SessionSecuredRequest
	{
		public string OldPassword = string.Empty;
		public string NewPassword = string.Empty;
	}
}
