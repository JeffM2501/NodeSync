using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonMessages
{
	public class JsonMessage
	{
		public string MessageName = string.Empty;

		public JsonMessage()
		{
			MessageName = this.GetType().ToString();
		}

        public virtual bool IsError ()
        {
            return false;
        }
	}
}
