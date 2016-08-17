using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace NodeController
{
    public class NodeIOLink
    {
        protected Process NodeProc = null;
        protected List<NodeIO.Message> InboundMessages = new List<NodeIO.Message>();

        public event EventHandler MessageReceived = null;

        public void Startup(string path)
        {
            Startup(path, new string[0]);
        }

        public void Startup(string path, string[] args)
        {
            NodeProc = new Process();
            NodeProc.StartInfo.FileName = path;
            NodeProc.StartInfo.Arguments = string.Join(" ",args);

            NodeProc.StartInfo.UseShellExecute = false;
            NodeProc.StartInfo.RedirectStandardInput = true;
            NodeProc.StartInfo.RedirectStandardOutput = true;

            NodeProc.OutputDataReceived += NodeProc_OutputDataReceived;

            NodeProc.Start();

            NodeProc.BeginOutputReadLine();

           // MessageThread = new Thread(new ThreadStart(CheckMessages));
        }

        public bool Alive
        {
            get { return NodeProc != null && !NodeProc.HasExited; }
        }

        public void Shutdown()
        {
            if (NodeProc != null)
            {
                NodeProc.CancelOutputRead();
                SendMessage(NodeIO.NodeShutdownMessage.ControllerKill);
                NodeProc.WaitForExit(100);
                if (!NodeProc.HasExited)
                    NodeProc.Kill();
            }

            NodeProc = null;
        }

        public void SendMessage(NodeIO.Message msg)
        {
            if (NodeProc == null)
                return;

            NodeProc.StandardInput.WriteLineAsync(NodeIO.MessageProcessor.PackMessage(msg));
        }

        public NodeIO.Message PopMessage()
        {
            lock (InboundMessages)
            {
                NodeIO.Message msg = null;

                if (InboundMessages.Count > 0)
                {
                    msg = InboundMessages[0];
                    InboundMessages.RemoveAt(0);
                }
                return msg;
            }
        }

        private void NodeProc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            NodeIO.Message msg = NodeIO.MessageProcessor.ParseMessage(e.Data);
            if (msg != null)
            {
                lock(InboundMessages)
                {
                    InboundMessages.Add(msg);
                }

                MessageReceived?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
