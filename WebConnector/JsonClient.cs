using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

using JsonMessages;
using System.IO;

namespace WebConnector
{
    public class JsonClient
    {
        protected class PendingJob
        {
            public WebRequest Requester = null;
            public JsonMessage Request = null;
            public object MessagToken = null;
        }

        protected List<PendingJob> Jobs = new List<PendingJob>();
      
        private string BaseURL = string.Empty;

        public class JsonMessageResponceArgs : EventArgs
        {
            public JsonMessage ResponceMessage = null;
            public JsonMessage RequestMessage = null;

            public object Token = null;
        }

        public event EventHandler<JsonMessageResponceArgs> ReceivedResponce = null;

        public JsonClient(string url)
        {
            BaseURL = url;
        }

        public bool SendMessage(JsonMessage request, object token)
        {
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(MessageProcessor.PackMessage(request));

            PendingJob job = new PendingJob();
            job.MessagToken = token;
            job.Request = request;
           // job.
            lock (Jobs)
            {
                if (Requester != null)
                    return false;

                MessagToken = token;
                Request = request;

                Requester = WebRequest.Create(BaseURL);

                Requester.ContentType = "application/json";
                Requester.Method = "POST";
                Requester.ContentLength = buffer.Length;

                var stream = Requester.GetRequestStream();
                stream.Write(buffer, 0, buffer.Length);
                stream.Close();

                Requester.BeginGetResponse(BeginGetResponseAsyncCallback, null);


            }
        }

        protected void BeginGetResponseAsyncCallback(IAsyncResult ar)
        {
            JsonMessageResponceArgs args = new JsonMessageResponceArgs();
            if (ar.IsCompleted)
            {
                string outString = string.Empty;
                lock(BDL)
                {
                    if (Requester == null)
                        return;

                    args.RequestMessage = Request;
                    args.Token = MessagToken;

                    var resp = Requester.EndGetResponse(ar);
                    var os = resp.GetResponseStream();
                    var sr = new StreamReader(os);
                    outString = sr.ReadToEnd();
                    sr.Close();
                    os.Close();

                    Requester = null;
                    Request = null;
                    MessagToken = null;
                }

                args.ResponceMessage = MessageProcessor.ParseMessage(outString);

                ReceivedResponce?.Invoke(this, args);
            }
        }

        public void Shutdown()
        {

        }
    }
}
