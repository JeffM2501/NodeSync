using System;
using System.Collections.Generic;
using System.Linq;
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

			public Dictionary<string, string> SessionItems = new Dictionary<string, string>();

			public string GetItem(string key)
			{
				lock(SessionItems)
				{
					if(SessionItems.ContainsKey(key))
						return SessionItems[key];

					return string.Empty;
				}
			}

			public void SetItem(string key, string value)
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

			public string GetSessionData(string id, string key)
			{
				lock(ActiveSessions)
				{
					if(ActiveSessions.ContainsKey(id))
						return ActiveSessions[id].GetItem(key);

					return string.Empty;
				}
			}

			public void SetSessionData(string id, string key, string value)
			{
				lock(ActiveSessions)
				{
					if(ActiveSessions.ContainsKey(id))
						ActiveSessions[id].SetItem(key, value);
				}
			}
		}

		protected SessionManager Sessions = new SessionManager();

		public delegate JsonMessage GetResponceMessageCB(JsonMessage request, SessionManager sessions);

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
				JsonMessage inboundMessage = new JsonMessage();

				// decode inbound

				context.Request.InputStream

				JsonMessage outBoundMessage = new JsonMessage(); // make this an error
				if(MessageCallback != null)
					outBoundMessage = MessageCallback(inboundMessage, Sessions);

				// encode outbound

				//context.Response
			}
		}

		protected override WebRequest GetRequestProcessor(HttpListenerContext context, GenerateWebResponceCB cb)
		{
			return new JsonMessageProcessor(context, MessageProcessor, Sessions);
		}
	}
}
