using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using System.Net;
using System.Net.Sockets;

namespace WebListener
{
    public class Host
    {
        protected HttpListener Listener = null;

        protected Thread ListenerThread = null;

        public List<string> Prefixes = new List<string>();

        public delegate void GenerateWebResponceCB(HttpListenerContext context);

        public GenerateWebResponceCB RequestHandler = DefaultRequetHandler;

        protected class WebRequest
        {
            public Thread ProcessThred = null;

            protected HttpListenerContext Request = null;

            protected GenerateWebResponceCB Callback = null;

            public event EventHandler RequetCompleted = null; 

            public WebRequest(HttpListenerContext context, GenerateWebResponceCB cb)
            {
                Request = context;
                ProcessThred = new Thread(new ThreadStart(GetResponce));
              
            }

            public void Start()
            {
                ProcessThred.Start();
            }

            protected void GetResponce()
            {
                if (Callback != null)
                     Callback(Request);

                RequetCompleted?.Invoke(this, EventArgs.Empty);
            }
        }

        protected List<WebRequest> RequestProcesses = new List<WebRequest>();

        public void Startup()
        {
            Shutdown();

            if (Prefixes.Count == 0)
            {
                if (System.Environment.OSVersion.Platform == PlatformID.Unix)
                    Prefixes.Add("http://*:8080");
                else
                    Prefixes.Add("http://*:80");
            }

            Listener = new HttpListener();
            foreach(string p in Prefixes)
                Listener.Prefixes.Add(p);

            Listener.Start();

            ListenerThread = new Thread(new ThreadStart(ProcessConnections));

        }

        public void Shutdown()
        {
            if (ListenerThread != null)
                ListenerThread.Abort();
            ListenerThread = null;

            lock(RequestProcesses)
            {
                foreach (var t in RequestProcesses)
                    t.ProcessThred.Abort();

                RequestProcesses.Clear();
            }

            if (Listener != null)
                Listener.Stop();

            Listener = null;
        }

        private static void DefaultRequetHandler(HttpListenerContext context)
        {
            context.Response.StatusCode = 500;
        }

        protected void ProcessConnections()
        {
            while (true)
            {
                WebRequest processor = new WebRequest(Listener.GetContext(), RequestHandler);
                processor.RequetCompleted += Processor_RequetCompleted;
                lock(RequestProcesses)
                {
                    RequestProcesses.Add(processor);
                    processor.Start();
                }
            }
        }

        private void Processor_RequetCompleted(object sender, EventArgs e)
        {
            WebRequest processor = sender as WebRequest;
            if (processor != null)
            {
                lock (RequestProcesses)
                    RequestProcesses.Remove(processor);
            }
        }
    }
}
