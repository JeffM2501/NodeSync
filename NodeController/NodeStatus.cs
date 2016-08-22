using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace NodeController
{
	public class NodeStatus
	{
		public Process ProcHandle = null;

		public string GlobalID = string.Empty;
		public string PublicName = string.Empty;

		public Dictionary<string, string> Properties = new Dictionary<string, string>();

		//protected List<string>
	}
}
