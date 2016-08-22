using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonMessages.MessageTypes
{
    public class TokenAuthenticatedRequest : JsonMessage
    {
        public string HostID = string.Empty;
        public string Token = string.Empty;
    }
}
