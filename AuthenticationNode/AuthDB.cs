using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.SQLite;

namespace AuthenticationNode
{
	public class AuthDB
	{
		protected SQLiteConnection DBConnection = null;
		protected string DBLocation = string.Empty;

		protected Random RNG = new Random();

		public virtual void Startup(string connectData)
		{
			DBLocation = connectData;

			DBConnection = new SQLiteConnection("Data Source=" + DBLocation + ";Version=3");
			DBConnection.Open();
		}

		public virtual void Shutdown()
		{
			if (DBConnection != null)
				DBConnection.Close();
		}

		public virtual bool CheckEmailExists(string email)
		{
			return GetUserIDFromEmail(email) != string.Empty;
		}

		public virtual string GetUserIDFromEmail(string email)
		{
			lock(DBConnection)
			{
				string sql = "SELECT UserID FROM users WHERE Email = @email";
				SQLiteCommand command = new SQLiteCommand(sql, DBConnection);
				command.Parameters.Add(new SQLiteParameter("@email", email));
				SQLiteDataReader reader = command.ExecuteReader();

				if(!reader.HasRows)
					return string.Empty;

				reader.Read();
				return reader.GetString(0);
			}
		}

		public class AuthenticationInformation
		{
			public string UserID = string.Empty;
			public string TokenSalt = string.Empty;
		}

		public virtual AuthenticationInformation GetAuthFromEmail(string email)
		{
			AuthenticationInformation auth = new AuthenticationInformation();

			auth.UserID = GetUserIDFromEmail(email);
			auth.TokenSalt = GetTokenSaltFromUID(auth.UserID);
			if(auth.UserID == string.Empty || auth.TokenSalt == string.Empty)
				return null;
	
			return auth;
		}

		public virtual string GetTokenSaltFromUID(string uid)
		{
			lock(DBConnection)
			{
				string sql = "SELECT ToakenSalt FROM authentication WHERE UserID = @uid AND Active = 1";
				SQLiteCommand command = new SQLiteCommand(sql, DBConnection);
				command.Parameters.Add(new SQLiteParameter("@uid", uid));
				SQLiteDataReader reader = command.ExecuteReader();

				if(!reader.HasRows)
					return string.Empty;

				return reader.GetString(0);
			}
		}

		protected bool CheckUIDExists(string uid)
		{
			lock(DBConnection)
			{
				string sql = "SELECT ID from users WHERE UserID = @UID";
				SQLiteCommand command = new SQLiteCommand(sql, DBConnection);
				command.Parameters.Add(new SQLiteParameter("@UID", uid));
				SQLiteDataReader reader = command.ExecuteReader();

				return reader.HasRows;
			}
		}

		protected virtual string MakeUID()
		{
			string uid = RNG.Next().ToString() + RNG.Next().ToString();
			while(CheckUIDExists(uid))
				uid = RNG.Next().ToString() + RNG.Next().ToString();

			return uid;
		}
		public class NewDBUserResults
		{
			public string UserID = string.Empty;
			public string EmailToken = string.Empty;
		}

		public virtual NewDBUserResults AddUser(string email, string passhash, string salt)
		{
			if(CheckEmailExists(email))
				return null;

			NewDBUserResults results = new NewDBUserResults();

			results.UserID = MakeUID();
			results.EmailToken = RNG.Next().ToString();

			lock(DBConnection)
			{
				string sql = "INSERT INTO users (UserID, Email, Active) values (@uid, @email, 0)";
				SQLiteCommand command = new SQLiteCommand(sql, DBConnection);
				command.Parameters.Add(new SQLiteParameter("@uid", results.UserID));
				command.Parameters.Add(new SQLiteParameter("@email", email));
				command.ExecuteNonQuery();

				sql = "INSERT INTO authentication (UserID, PassHash, TokenSalt, Active) values (@uid, @passhash, @salt, 1)";
				command = new SQLiteCommand(sql, DBConnection);
				command.Parameters.Add(new SQLiteParameter("@uid", results.UserID));
				command.Parameters.Add(new SQLiteParameter("@passhash", passhash));
				command.Parameters.Add(new SQLiteParameter("@salt", salt));
				command.ExecuteNonQuery();

				sql = "INSERT INTO validations (UserID, Token, SentOn, Validated) values (@uid, @token, @sent, 0)";
				command = new SQLiteCommand(sql, DBConnection);
				command.Parameters.Add(new SQLiteParameter("@uid", results.UserID));
				command.Parameters.Add(new SQLiteParameter("@token", results.EmailToken));
				command.Parameters.Add(new SQLiteParameter("@sent", DateTime.UtcNow));
				command.ExecuteNonQuery();
			}

			return results;
		}

		public virtual bool ValidateEmailToken(string userID, string token, DateTime lowerLimit)
		{
			DateTime dbTokenTime = DateTime.MinValue;
			int id = int.MinValue;

			lock(DBConnection)
			{
				string sql = "SELECT ID, SentOn FROM validations WHERE UserID = @userID AND Token = @token AND Validated = 0";
				SQLiteCommand command = new SQLiteCommand(sql, DBConnection);
				command.Parameters.Add(new SQLiteParameter("@userID", userID));
				command.Parameters.Add(new SQLiteParameter("@token", token));
				SQLiteDataReader reader = command.ExecuteReader();

				if(!reader.HasRows)
					return false;

				reader.Read();
				id = reader.GetInt32(0);
				dbTokenTime = reader.GetDateTime(1);
			}

			if(dbTokenTime < lowerLimit)
				return false;

			lock(DBConnection)
			{
				string sql = "Update validations SET Validated=1 ValidatedOn=@now WHERE ID = @id";
				SQLiteCommand command = new SQLiteCommand(sql, DBConnection);
				command.Parameters.Add(new SQLiteParameter("@now", DateTime.UtcNow));
				command.Parameters.Add(new SQLiteParameter("@id", id));
				command.ExecuteNonQuery();
			}

			return true;

		}

		/// <summary>
		/// Validates a user by email and password hash, returns the userID and token encryption key
		/// </summary>
		/// <param name="email"></param>
		/// <param name="passhsh"></param>
		/// <returns></returns>
		public virtual bool ValidateUser(string uid, string passhsh)
		{
			lock(DBConnection)
			{
				string sql = "SELECT ID FROM authentication WHERE UserID = @uid AND PassHash = @passhsh AND Active = 1";
				SQLiteCommand command = new SQLiteCommand(sql, DBConnection);
				command.Parameters.Add(new SQLiteParameter("@uid", uid));
				command.Parameters.Add(new SQLiteParameter("@passhsh", passhsh));
				SQLiteDataReader reader = command.ExecuteReader();

				if(!reader.HasRows)
					return false;

				int id = int.MinValue;
				string salt = string.Empty;
				reader.Read();
				id = reader.GetInt32(0);

				sql = "Update authentication SET LastAuthentication=@now WHERE ID = @id";
				command = new SQLiteCommand(sql, DBConnection);
				command.Parameters.Add(new SQLiteParameter("@now", DateTime.UtcNow));
				command.Parameters.Add(new SQLiteParameter("@id", id));
				command.ExecuteNonQuery();
				
				return true;
			}
		}
	}
}
