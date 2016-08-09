using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.IO;

namespace JsonMessages
{
	public static class MessageProcessor
	{
		private static Dictionary<string, Type> MessageTypes = new Dictionary<string, Type>();

		static MessageProcessor()
		{
			AddMessages(Assembly.GetExecutingAssembly());
			if (Assembly.GetCallingAssembly() != Assembly.GetExecutingAssembly())
				AddMessages(Assembly.GetCallingAssembly());
		}

		public static void AddMessages(Assembly assemby)
		{
			foreach(var t in assemby.GetTypes())
				AddMessage(t);
		}

		public static void AddMessage(JsonMessage message)
		{
			AddMessage(message.GetType());
		}

		public static void AddMessage(Type t)
		{
			if (t.IsSubclassOf(typeof(JsonMessage)))
			{
				lock (MessageTypes)
				{
					if(MessageTypes.ContainsKey(t.Name))
						MessageTypes[t.Name] = t;
					else
						MessageTypes.Add(t.Name, t);
				}
			}
		}

		public static Type GetMessageType(string name)
		{
			lock(MessageTypes)
			{
				if(MessageTypes.ContainsKey(name))
					return MessageTypes[name];
			}
			return null;
		}

		public static JsonMessage ParseMessage(string input)
		{
			GeneralErrorMessage defaultError = GeneralErrorMessage.ParseError;

			byte[] buffer = System.Text.Encoding.Unicode.GetBytes(input);
			MemoryStream ms = new MemoryStream(buffer);

			DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(JsonMessage));
			var baseMessage = ser.ReadObject(ms) as JsonMessage;
			if (baseMessage != null)
            {
                ms.Close();
                ms = new MemoryStream(buffer);// reset for re-parse in final type

                Type t = GetMessageType(baseMessage.MessageName);

                if (t != null)
                {
                    ser = new DataContractJsonSerializer(t);

                    JsonMessage outMsg = ser.ReadObject(ms) as JsonMessage;
                    if (outMsg != null)
                        return outMsg;
                }
                else
                    defaultError = GeneralErrorMessage.UnknownError;

                ms.Close();
            }

            return defaultError;
        }

        public static string PackMessage(JsonMessage message)
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(message.GetType());
            MemoryStream ms = new MemoryStream();
            ser.WriteObject(ms, message);
            ms.Close();

            return System.Text.Encoding.UTF8.GetString(ms.GetBuffer());
        }
	}
}
