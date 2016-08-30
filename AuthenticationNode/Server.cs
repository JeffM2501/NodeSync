using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using System.Net.Mail;

using System.Security.Cryptography;

using JsonMessages;
using JsonMessages.Authentication;
using JsonMessages.Authentication.Peers;
using WebListener;
using WebConnector;
using System.Net;

using EncodingTools;

namespace AuthenticationNode
{
	public class Server : JsonMessageHost
	{
		protected AuthDB DB = null;

		protected AuthConfig Config = null;

		public class PeerConnection
		{
			public string Name = string.Empty;

			public JsonClient Connection = null;
			public List<string> PendingUpdates = new List<string>();

			protected RijndaelManaged Crypto = null;
			protected ICryptoTransform Encrypter = null;

			protected Thread Worker = null;
			protected AuthDB DB = null;

			public PeerConnection(string name, string host, RijndaelManaged c, AuthDB db)
			{
				Connection = new JsonClient(host);

				Name = name;
				Crypto = c;
				DB = db;

				if (Crypto != null)
					Encrypter = Crypto.CreateEncryptor();
			}

			public void Add(string uid)
			{
				lock(PendingUpdates)
					PendingUpdates.Add(uid);

				if (Worker == null)
				{
					Worker = new Thread(new ThreadStart(Process));
					Worker.Start();
				}
			}

			private string PopUserID()
			{
				string id = string.Empty;
				lock(PendingUpdates)
				{
					if (PendingUpdates.Count > 0)
					{
						id = PendingUpdates[0];
						PendingUpdates.RemoveAt(0);
					}
				}
				return id;
			}

			private void Process()
			{
				string id = PopUserID();
				while (id != string.Empty)
				{
					if (Crypto != null)
					{
						PeerAddUserRequest request = new PeerAddUserRequest();

						request.UserID = id;
						request.TokenSalt = DB.GetTokenSaltFromUID(id);
						request.PassHash = DB.GetPassHashFromID(id);
						request.Email = DB.GetEmailFromID(id);

						request.Name = Name;

						MemoryStream ms = new MemoryStream();
						StreamWriter sw = new StreamWriter(new CryptoStream(ms, Encrypter, CryptoStreamMode.Write));
						sw.Write(request.UserID);
						sw.Close();
						ms.Close();

						request.Key = Convert.ToBase64String(ms.GetBuffer());

						Connection.SendMessage(request, null, null);
					}
					id = PopUserID();
				}
			}
		}

        protected Dictionary<string, PeerConnection> PeerConnections = new Dictionary<string, PeerConnection>();

		public Server(AuthConfig cfg)
		{
			Config = cfg;

			Prefixes = Config.ListenPrefixes;
			DB = GetAuthDB();
			DB.Startup(cfg.AuthDBLocation);

			this.MessageProcessor = ProcessAuthMessage;

			foreach(var o in cfg.OutboundUpdatePeers)
			{
				var p = new PeerConnection(o.Name, o.Host, Encryption.BuildCrypto(o.APIKey), DB);
				PeerConnections.Add(o.Name, p);
			}
		}

		protected AuthDB GetAuthDB()
		{
			return new AuthDB();    // TODO, check plugins for auth DB handlers
		}

		public override void Shutdown()
		{
			DB.Shutdown();
			base.Shutdown();
		}

		internal virtual bool TimeToQuit()
		{
			return false;
		}

		// timeout protected session data
		protected readonly string DateSessionFieldName = "AUTH_SERVER_LAST_SESSION_DATE";
		protected readonly string ValidLoginString = "ValidLogin";

		public static int SessionTimeOutSeconds = 300;
		public static int TokenTimeoutMinutes = 5;

		protected virtual string CreateSession()
		{
			string session = this.Sessions.CreateSession();

			Sessions.SetSessionData(session, DateSessionFieldName, DateTime.Now);
			return session;
		}

		protected virtual object GetSessionData(string id, string key)
		{
			object l = Sessions.GetSessionData(id, DateSessionFieldName);
			DateTime lastUpdate = l == null ? DateTime.MinValue : (DateTime)l;

			if(lastUpdate == null || (DateTime.Now - lastUpdate).Seconds > SessionTimeOutSeconds)
			{
				Sessions.ClearSessionData(id);
				return null;
			}

			Sessions.SetSessionData(id, DateSessionFieldName, DateTime.Now);
			return Sessions.GetSessionData(id, key);
		}

		protected string GetSessionDataS(string id, string key)
		{
			object l = GetSessionData(id,key);
			return l == null ? string.Empty : l as string;
		}

		protected virtual void SetSessionData(string id, string key, object value)
		{
			Sessions.SetSessionData(id, DateSessionFieldName, DateTime.Now);
			Sessions.SetSessionData(id, key, value);
		}

		// message processing
		protected JsonMessage ProcessAuthMessage(JsonMessage request, JsonMessageHost.SessionManager sessions)
		{
			if(request as SessionSecuredRequest != null)
			{
				string uid = GetSessionDataS((request as SessionSecuredRequest).SessionID, ValidLoginString);

				if(uid != (request as SessionSecuredRequest).UserID)	// make sure they are the same person we think they are
				{
					Sessions.ClearSessionData((request as SessionSecuredRequest).SessionID);
					return GeneralErrorMessage.SessionError;
				}
			}

			JsonMessage responce = null;
			if(request as CreateUserRequest != null)
				responce = CreateUser(request as CreateUserRequest);
			else if(request as LoginUserRequest != null)
				responce = LoginUser(request as LoginUserRequest);
			else if(request as ValidateEmailTokenRequest != null)
				responce = ValidateEmailToken(request as ValidateEmailTokenRequest);
			else if(request as ChangePasswordRequest != null)
				responce = ChangePassword(request as ChangePasswordRequest);
			else if(request as ValidateAuthenticationTokenRequest != null)
				responce = ValidateAuthenticationToken(request as ValidateAuthenticationTokenRequest);
            else if (request as PeerAddUserRequest != null)
                responce = PeerAddUser(request as PeerAddUserRequest);

            // ensure the session ID gets passed on
            if (responce as SessionSecuredResponce != null && request as SessionSecuredRequest != null)
				(responce as SessionSecuredResponce).SessionID = (request as SessionSecuredRequest).SessionID;

			return responce;
		}

		protected JsonMessage CreateUser(CreateUserRequest request)
		{
			CreateUserResponce responce = new CreateUserResponce();
			responce.OK = false;
			responce.Responce = "Invalid";

			if(request != null && Config.AllowRegistration)
			{
				if(!DB.CheckEmailExists(request.Email))
				{
					// build the salt
					RijndaelManaged crypto = new RijndaelManaged();
					crypto.BlockSize = 256;
					crypto.GenerateIV();
					crypto.GenerateKey();

					string key = Convert.ToBase64String(crypto.Key);
					string iv = Convert.ToBase64String(crypto.IV);

					string sharedKey = key + ":" + iv;

					var user = DB.AddUser(request.Email, HashPassword(request.Password, iv), sharedKey);

					// send the validation email
					Dictionary<string, string> args = new Dictionary<string, string>();
					args.Add("%NAME", request.Email);
					args.Add("%TOKEN", user.EmailToken);

					SendEmail(request.Email, "CreateUser", args);

					// tell them it worked
					responce.OK = true;
					responce.Responce = user.UserID;

					SendUserUpdate(user.UserID);
				}
			}
			return responce;
		}


		// this token generation is not super secure, it is just an example. Really should use the crypt to...crypto something
		// buy that's what plugins are for, so the exact crypto method used in live servers is unknown, and can be changed.

		protected virtual string GenerateAuthToken(string userID, RijndaelManaged crypto)
		{
			if(API.LastProcessor != null)
				return API.LastProcessor.GenerateAuthToken(userID, crypto);
			
			string mins = EncodingTools.UnixTime.GetTokenMinutes().ToString();

			string hash = HashPassword(mins, Convert.ToBase64String(crypto.Key));
			return mins + ":" + hash;
		}

		protected virtual bool ValidateAuthToken(string userID, string token, RijndaelManaged crypto)
		{
			if(API.LastProcessor != null)
				return API.LastProcessor.ValidateAuthToken(userID, token, crypto);

			string[] parts = token.Split(":".ToCharArray(), 2);
			if(parts.Length != 2)
				return false;

			string hash = HashPassword(parts[1], Convert.ToBase64String(crypto.Key));

			if(hash != parts[1])
				return false;

			int time = int.MinValue;
			if(!int.TryParse(parts[0], out time))
				return false;

			if(Math.Abs(time - EncodingTools.UnixTime.GetTokenMinutes()) > TokenTimeoutMinutes)
				return false;

			return true;
		}

		protected RijndaelManaged CheckPassword(string userID, string tokenSalt, string password)
		{
				// build the salt
			RijndaelManaged crypto = Encryption.BuildCrypto(tokenSalt);
			if (crypto != null)
			{
				if(DB.ValidateUser(userID, HashPassword(password, Convert.ToBase64String(crypto.IV))))
					return crypto;
			}
			return null;
		}

		protected JsonMessage LoginUser(LoginUserRequest request)
		{
			LoginUserResponce responce = new LoginUserResponce();
			responce.OK = false;
			responce.Responce = "Invalid";

			if(request != null)
			{
				var login = DB.GetAuthFromEmail(request.Email);
				if (login != null)
				{
					var crypto = CheckPassword(login.UserID, login.TokenSalt, request.Password);
					if (crypto != null)
					{
						responce.OK = true;
						responce.UserID = login.UserID;

						responce.Responce = GenerateAuthToken(login.UserID, crypto);

						responce.SessionID = CreateSession();
					}
				}
			}
			return responce;
		}

		protected void SendEmail(string to, string template, Dictionary<string,string> args)
		{
			var email = Config.FindTemplate(template);

			string body = email.Body;
			string subject = email.Subject;

			foreach( KeyValuePair<string,string> a in args)
			{
				body = body.Replace(a.Key, a.Value);
				subject = subject.Replace(a.Key, a.Value);
			}

			SmtpClient smtp = new SmtpClient(Config.MailSMTPServer);
			if (Config.SMTPUsername != string.Empty)
			{
				smtp.EnableSsl = true;
				smtp.Credentials = new NetworkCredential(Config.SMTPUsername, Config.SMTPPassword);
			}

			smtp.SendAsync(Config.SMTPFrom, to, subject, body, null);
		}

		protected ValidateEmailTokenResponce ValidateEmailToken(ValidateEmailTokenRequest request)
		{
			ValidateEmailTokenResponce responce = new ValidateEmailTokenResponce();
			responce.OK = false;
			responce.Responce = "Invalid";
			responce.SessionID = request.SessionID;

			if(request != null && Config.AllowRegistration)
			{
				// email tokens are good for a day
				if (DB.ValidateEmailToken(request.UserID, request.EmailToken, (DateTime.UtcNow - new TimeSpan(1, 0, 0, 0))))
				{
					responce.OK = true;
					responce.Responce = "Valid";
					SendUserUpdate(request.UserID);
				}
			}

			return responce;
		}

		protected ChangePasswordResponce ChangePassword(ChangePasswordRequest request)
		{
			ChangePasswordResponce responce = new ChangePasswordResponce();
			responce.OK = false;

            if (Config.AllowRegistration && request != null)
            {
                string uid = GetSessionDataS(request.SessionID, ValidLoginString);

                string tokenSalt = DB.GetTokenSaltFromUID(uid);

                var crypto = CheckPassword(uid, tokenSalt, request.OldPassword);
                if (crypto != null)
                {
                    string newHash = HashPassword(request.NewPassword, Convert.ToBase64String(crypto.IV));
                    if (DB.UpdateUserPassword(uid, newHash))
                    {
                        responce.OK = true;
                        responce.Responce = "Updated";

						SendUserUpdate(uid);
					}
                    else
                        responce.Responce = "Invalid New Password";
                }
                else
                    responce.Responce = "Invalid Credentials";
            }

            return responce;
		}

		protected ValidateAuthenticationTokenResponce ValidateAuthenticationToken(ValidateAuthenticationTokenRequest request)
		{
			ValidateAuthenticationTokenResponce responce = new ValidateAuthenticationTokenResponce();
			responce.OK = false;
			responce.Responce = "Invalid";

			string tokenSalt = DB.GetTokenSaltFromUID(request.UserID);

			var crypto = Encryption.BuildCrypto(tokenSalt);

			if (ValidateAuthToken(request.UserID, request.Token, crypto))
			{
				responce.OK = true;
				responce.Responce = "Valid";
			}

			return responce;
		}

		protected string HashPassword(string pass, string salt)
		{
			var hasher = SHA256.Create();
			string toHash = pass + salt;

			return Convert.ToBase64String(hasher.ComputeHash(Encoding.UTF8.GetBytes(toHash)));
		}

        protected PeerResponce PeerAddUser(PeerAddUserRequest request )
        {
            PeerResponce responce = new PeerResponce();
            responce.OK = false;

            if (request != null)
            {
                AuthConfig.APIPeer inPeer = Config.InboundUpdatePeers.Find(x => x.Name == request.Name);

                if(inPeer != null)
                {
                    RijndaelManaged crypto = Encryption.BuildCrypto(inPeer.APIKey);

                    MemoryStream ms = new MemoryStream(Convert.FromBase64String(request.Key));
                    StreamReader sr = new StreamReader(new CryptoStream(ms, crypto.CreateDecryptor(), CryptoStreamMode.Read));
                    string decodeKey = sr.ReadToEnd();
                    sr.Close();
                    ms.Close();

                    if (decodeKey == request.UserID)    // they know the secret!
                    {
                        DB.UpdateUserInfo(request.UserID, request.Email, request.PassHash, request.TokenSalt);
                        responce.OK = true;
                    }
                }
            }

            return responce;
        }

        protected void SendUserUpdate(string userID)
        {
			foreach(var peer in PeerConnections)
				peer.Value.Add(userID);
        }
    }
}
