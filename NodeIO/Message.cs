using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeIO
{
    public class Message
    {
        public string Name = string.Empty;

        public Message()
        {
            Name = this.GetType().Name;
        }
    }
}
