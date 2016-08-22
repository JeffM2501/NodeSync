using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonMessages.MessageTypes.NodeStatus
{
    public class NodeStatusResponce : TokenAuthenticatedResponce
    {
        public class NodeInfo
        {
            public string Name = string.Empty;
            public string ID = string.Empty;
        }
        public List<NodeInfo> ActiveNodes = new List<NodeInfo>();

        public double Utilization = 0;
    }
}
