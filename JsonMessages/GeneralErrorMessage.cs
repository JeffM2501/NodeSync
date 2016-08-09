using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonMessages
{
	public class GeneralErrorMessage : JsonMessage
	{
		public string ErrorCode = string.Empty;
		public string ErrorMessage = string.Empty;

		public GeneralErrorMessage() : base()
		{

		}

		public GeneralErrorMessage(string code, string msg) : base()
		{
			ErrorCode = code;
			ErrorMessage = msg;
		}

		public static readonly GeneralErrorMessage ParseError = new GeneralErrorMessage("000", "Message Parse Error");
	}
}
