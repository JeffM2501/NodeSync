using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Net;

using JsonMessages;

namespace WebListener
{
	public class JsonMessageHost : Host
	{
		internal class SessionData
		{
			public string SessionID = string.Empty;

			public Dictionary<string, object> SessionItems = new Dictionary<string, object>();

			public object GetItem(string key)
			{
				lock(SessionItems)
				{
					if(SessionItems.ContainsKey(key))
						return SessionItems[key];

					return null;
				}
			}

			public void SetItem(string key, object value)
			{
				lock(SessionItems)
				{
					if(SessionItems.ContainsKey(key))
						SessionItems.Add(key, value);
					else
						SessionItems[key] = value;
				}
			}
		}

		public class SessionManager
		{
			internal Dictionary<string, SessionData> ActiveSessions = new Dictionary<string, SessionData>();
			private Random RNG = new Random();

			protected string GenSessionKey()
			{
				return RNG.Next().ToString() + RNG.Next().ToString() + RNG.Next().ToString();
			}

			public string CreateSession()
			{
				lock(ActiveSessions)
				{
					string key = GenSessionKey();
					while(ActiveSessions.ContainsKey(key))
						key = GenSessionKey();

					SessionData sd = new SessionData();
					sd.SessionID = key;

					ActiveSessions.Add(key, sd);

					return key;
				}
			}

			public void ClearSessionData(string id)
			{
				lock(ActiveSessions)
				{
					if(ActiveSessions.ContainsKey(id))
						ActiveSessions.Remove(id);
				}
			}

			public object GetSessionData(string id, string key)
			{
				lock(ActiveSessions)
				{
					if(ActiveSessions.ContainsKey(id))
						return ActiveSessions[id].GetItem(key);

					return string.Empty;
				}
			}

			public void SetSessionData(string id, string key, object value)
			{
				lock(ActiveSessions)
				{
					if(ActiveSessions.ContainsKey(id))
						ActiveSessions[id].SetItem(key, value);
				}
			}
		}

		protected SessionManager Sessions = new SessionManager();

		public delegate JsonMessage GetResponceMessageCB(JsonMessage request, JsonMessageHost.SessionManager sessions);

		public GetResponceMessageCB MessageProcessor = null;

		protected class JsonMessageProcessor : Host.WebRequest
		{
			protected GetResponceMessageCB MessageCallback = null;
			protected SessionManager Sessions = null;

			public JsonMessageProcessor(HttpListenerContext context, GetResponceMessageCB cb, SessionManager sessions) : base(context,null) 
			{
				Callback = ProcessJsonRequest;
				Sessions = sessions;
			}

			protected virtual void ProcessJsonRequest(HttpListenerContext context)
			{
                // decode inbound
                StreamReader reader = new StreamReader(context.Request.InputStream);
                var inboundMessage = JsonMessages.MessageProcessor.ParseMessage(reader.ReadToEnd());
                reader.Close();

                JsonMessage outBoundMessage = null;

                // if we unparsed an error just send it back
                if (inboundMessage.IsError())
                    outBoundMessage = inboundMessage;
                else
                {   // see what they want to do with it
                    if (MessageCallback != null)
                        outBoundMessage = MessageCallback(inboundMessage, Sessions);
                }

                // send the response back
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Response.ContentType = "application/json";

                string outText = JsonMessages.MessageProcessor.PackMessage(outBoundMessage);

                StreamWriter sw = new StreamWriter(context.Response.OutputStream);
                sw.Write(outText);
                sw.Close();
            }
		}

		protected override WebRequest GetRequestProcessor(HttpListenerContext context, GenerateWebResponceCB cb)
		{
			return new JsonMessageProcessor(context, MessageProcessor, Sessions);
		}
	}
}
