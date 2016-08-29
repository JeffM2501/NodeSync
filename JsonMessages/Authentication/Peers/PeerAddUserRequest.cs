using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonMessages.Authentication.Peers
{
    public class PeerAddUserRequest : JsonMessage
    {
        public string Name = string.Empty;
        public string Key = string.Empty;
        public string UserID = string.Empty;
        public string Email = string.Empty;
        public string PassHash = string.Empty;
        public string TokenSalt = string.Empty;
    }
}
