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

        public override bool IsError()
        {
            return true;
        }

        public static readonly GeneralErrorMessage ParseError = new GeneralErrorMessage("000", "Message Parse Error");
        public static readonly GeneralErrorMessage UnknownError = new GeneralErrorMessage("001", "Unknown Message Error");
		public static readonly GeneralErrorMessage SessionError = new GeneralErrorMessage("002", "Session Timeout");
	}
}
