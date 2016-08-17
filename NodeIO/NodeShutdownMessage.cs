using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeIO
{
    public class NodeShutdownMessage : Message
    {
        public string Reason = string.Empty;

        public NodeShutdownMessage() : base() { }
        public NodeShutdownMessage(string reason) : base()
        {
            Reason = reason;
        }

        public static readonly NodeShutdownMessage ControllerKill = new NodeShutdownMessage("Node Controller Forced Shutdown");
    }
}
