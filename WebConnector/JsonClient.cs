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
		public class JsonMessageResponceArgs : EventArgs
		{
			public JsonMessage ResponceMessage = null;
			public JsonMessage RequestMessage = null;

			public object Token = null;
		}

		public event EventHandler<JsonMessageResponceArgs> ReceivedResponce = null;


		protected class PendingJob
        {
            public WebRequest Requester = null;
            public JsonMessage Request = null;
            public object MessagToken = null;

			public IAsyncResult AsyncResult = null;

			public event EventHandler<JsonMessageResponceArgs> ResponceCallback = null;

			public void CallResponce(JsonMessageResponceArgs args)
			{
				ResponceCallback?.Invoke(this, args);
			}
		}

        protected List<PendingJob> Jobs = new List<PendingJob>();
      
        private string BaseURL = string.Empty;

       
        public JsonClient(string url)
        {
			SetURL(url);
        }

		public void SetURL(string url)
		{
			BaseURL = url;
		}

		public void SendMessage(JsonMessage request, object token)
		{
			SendMessage(request, token, ReceivedResponce);
		}

		public void SendMessage(JsonMessage request, object token, EventHandler<JsonMessageResponceArgs> handler)
        {
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(MessageProcessor.PackMessage(request));

            PendingJob job = new PendingJob();
            job.MessagToken = token;
            job.Request = request;

			job.Requester  = WebRequest.Create(BaseURL);

			job.Requester.ContentType = "application/json";
			job.Requester.Method = "POST";
			job.Requester.ContentLength = buffer.Length;
			if (handler != null)
				job.ResponceCallback += handler;

			var stream = job.Requester.GetRequestStream();
			stream.Write(buffer, 0, buffer.Length);
			stream.Close();

			// job.
			lock(Jobs)
			{
				Jobs.Add(job);
				job.AsyncResult = job.Requester.BeginGetResponse(BeginGetResponseAsyncCallback, job);
			}
        }

        protected void BeginGetResponseAsyncCallback(IAsyncResult ar)
        {
            if (ar.IsCompleted)
            {
                string outString = string.Empty;
				PendingJob job = ar.AsyncState as PendingJob;
				if(job == null)
					return;

				lock(Jobs)
				{
					Jobs.Remove(job);
				}

				JsonMessageResponceArgs args = new JsonMessageResponceArgs();
				args.RequestMessage = job.Request;
				args.Token = job.MessagToken;

				var resp = job.Requester.EndGetResponse(ar);
				var os = resp.GetResponseStream();
				var sr = new StreamReader(os);
				outString = sr.ReadToEnd();
				sr.Close();
				os.Close();

				args.ResponceMessage = MessageProcessor.ParseMessage(outString);

				job.CallResponce(args);
            }
        }

        public void Shutdown()
        {
			lock(Jobs)
			{
				foreach(var job in Jobs)
				{
					if(job.AsyncResult != null)
						job.Requester.Abort();
				}

				Jobs.Clear();
			}
		}
    }
}
