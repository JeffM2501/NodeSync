using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace AuthenticationNode
{
	public class AuthConfig
	{
		public List<string> ListenPrefixes = new List<string>();

		public bool AllowRegistration = true;

		public class APIPeer
		{
			public string Name = string.Empty;
			public string Host = string.Empty;
			public string APIKey = string.Empty;
		}

		public List<APIPeer> OutboundUpdatePeers = new List<APIPeer>();
		public List<APIPeer> InboundUpdatePeers = new List<APIPeer>();

		public string AuthDBLocation = string.Empty;

		public string LogFolder = string.Empty;

		public string PlugInsFolder = string.Empty;

		public string MailSMTPServer = string.Empty;

		public string SMTPFrom = string.Empty;
		public string SMTPUsername = string.Empty;
		public string SMTPPassword = string.Empty;
	
		public class EmailTemplate
		{
			public string Name = string.Empty;
			public string Subject = "Unset Email Template";
			public string Body = "%NAME  %TOKEN";
			public bool UseHTML = false;
		}

		public List<EmailTemplate> EmailTemplates = new List<EmailTemplate>();

		public EmailTemplate FindTemplate(string name)
		{
			foreach(var t in EmailTemplates)
			{
				if(t.Name == name)
					return t;
			}

			return new EmailTemplate();
		}

		public void Save(string path)
		{
			try
			{
				FileInfo file = new FileInfo(path);
				if(file.Exists)
					file.Delete();

				FileStream fs = file.OpenWrite();
				new XmlSerializer(typeof(AuthConfig)).Serialize(fs, this);
				fs.Close();

			}
			catch (System.Exception /*ex*/)
			{
				
			}
			
		}

		public static AuthConfig Load(string path)
		{
			StreamReader sr = null;
			try
			{
				XmlSerializer xml = new XmlSerializer(typeof(AuthConfig));

				FileInfo file = new FileInfo(path);
				if(file.Exists)
				{
					sr = file.OpenText();
					AuthConfig cfg = xml.Deserialize(sr) as AuthConfig;

					sr.Close();
					if(cfg != null)
						return cfg;
				}
				else
				{
					FileStream fs = file.OpenWrite();
					xml.Serialize(fs, new AuthConfig());
					fs.Close(); 
				}
			}
			catch(System.Exception /*ex*/)
			{
				if(sr != null)
					sr.Close();
			}

			return new AuthConfig();
		}
	}
}
