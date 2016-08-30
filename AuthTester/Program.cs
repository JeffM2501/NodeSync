using System;
using System.Collections.Generic;
using System.Threading;

using WebConnector;
using JsonMessages.Authentication;


namespace AuthTester
{
	static class Program
	{
		static bool done = false;

		static JsonClient Client = new JsonClient("http://localhost:8080/");

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args )
		{
			while (!done)
			{
				Console.WriteLine("Enter Command");

				string command = Console.ReadLine();

				if(command == "quit")
					done = true;
				else if(command == "seturl")
					SetURL();
				else if(command == "register")
					Register();
				else if(command == "login")
					Login();
				else if(command == "change")
					ChangePassword();
				else if(command == "validateemail")
					ValidateEmailToken();
				else if(command == "validateauth")
					ValidateAuthToken();
			}
		}

		static void SetURL()
		{
			Console.WriteLine("Enter URL");
			Client.SetURL(Console.ReadLine());
			Console.WriteLine("URL Set");
			Console.WriteLine();
		}

		static object Locker = new object();
		static bool IsWaiting = false;

		static void SetWait()
		{
			lock(Locker)
				IsWaiting = true;
		}
		
		static void ClearWait()
		{
			lock(Locker)
				IsWaiting = false;
		}

		static bool Wait()
		{
			lock(Locker)
				return IsWaiting;
		}

		static void Register()
		{
			CreateUserRequest request = new CreateUserRequest();
			
			Console.WriteLine("Enter Email");
			request.Email = Console.ReadLine();
			
			Console.WriteLine("Enter Password");
			request.Password = Console.ReadLine();

			Console.WriteLine("Create Sent");
			Console.WriteLine();

			SetWait();
			Client.SendMessage(request, null, CreateReceived);

			while (Wait())
				Thread.Sleep(100);
		}

		private static void CreateReceived(object sender, JsonClient.JsonMessageResponceArgs e)
		{
			CreateUserResponce responce = e.ResponceMessage as CreateUserResponce;
			if(responce == null)
				Console.WriteLine("Invalid responce" + e.ResponceMessage.MessageName);
			else
			{
				Console.WriteLine(responce.OK ? "Create OK" : "Create Failed");
				Console.WriteLine(responce.Responce);
			}
			ClearWait();
		}

		public static string LastUserID = string.Empty;
		public static string LastSessionToken = string.Empty;
		public static string LastAuthToken = string.Empty;

		static void Login()
		{
			LoginUserRequest request = new LoginUserRequest();

			Console.WriteLine("Enter Email");
			request.Email = Console.ReadLine();

			Console.WriteLine("Enter Password");
			request.Password = Console.ReadLine();

			Console.WriteLine("Login Sent");
			Console.WriteLine();

			SetWait();
			Client.SendMessage(request, null, LoginReceived);

			while(Wait())
				Thread.Sleep(100);
		}

		private static void LoginReceived(object sender, JsonClient.JsonMessageResponceArgs e)
		{
			LastSessionToken = string.Empty;
			LastUserID = string.Empty;
			LastAuthToken = string.Empty;

			LoginUserResponce responce = e.ResponceMessage as LoginUserResponce;
			if(responce == null)
				Console.WriteLine("Invalid responce" + e.ResponceMessage.MessageName);
			else
			{
				if (responce.OK)
				{
					Console.WriteLine("Login OK");
					Console.WriteLine(responce.UserID);
					Console.WriteLine(responce.SessionID);
					Console.WriteLine(responce.Responce);
					LastUserID = responce.UserID;
					LastSessionToken = responce.SessionID;
					LastAuthToken = responce.Responce;
				}
				else
				{
					Console.WriteLine("Login Failed");
					Console.WriteLine(responce.Responce);
				}
			}

			ClearWait();
		}

		static void ChangePassword()
		{
			ChangePasswordRequest request = new ChangePasswordRequest();

			request.UserID = LastUserID;
			request.SessionID = LastSessionToken;

			Console.WriteLine("Enter Current Password");
			request.OldPassword = Console.ReadLine();

			Console.WriteLine("Enter Password");
			request.NewPassword = Console.ReadLine();

			Console.WriteLine("Password Change Sent");
			Console.WriteLine();

			SetWait();
			Client.SendMessage(request, null, ChangePasswordReceived);

			while(Wait())
				Thread.Sleep(100);
		}

		private static void ChangePasswordReceived(object sender, JsonClient.JsonMessageResponceArgs e)
		{
			LastSessionToken = string.Empty;

			ChangePasswordResponce responce = e.ResponceMessage as ChangePasswordResponce;
			if(responce == null)
				Console.WriteLine("Invalid responce" + e.ResponceMessage.MessageName);
			else
			{
				if(responce.OK)
				{
					Console.WriteLine("Password Change OK");
					Console.WriteLine(responce.Responce);
					Console.WriteLine(responce.SessionID);
					LastSessionToken = responce.SessionID;
				}
				else
				{
					Console.WriteLine("Password Change Failed");
					Console.WriteLine(responce.Responce);
				}
			}

			ClearWait();
		}
		static void ValidateEmailToken()
		{
			ValidateEmailTokenRequest request = new ValidateEmailTokenRequest();

			request.UserID = LastUserID;
			request.SessionID = LastSessionToken;

			Console.WriteLine("Enter Mail Token");
			request.EmailToken = Console.ReadLine();

			Console.WriteLine("Token Validation Sent");
			Console.WriteLine();

			SetWait();
			Client.SendMessage(request, null, ValidateEmailTokenReceived);

			while(Wait())
				Thread.Sleep(100);
		}

		private static void ValidateEmailTokenReceived(object sender, JsonClient.JsonMessageResponceArgs e)
		{
			ValidateEmailTokenResponce responce = e.ResponceMessage as ValidateEmailTokenResponce;
			if(responce == null)
				Console.WriteLine("Invalid responce" + e.ResponceMessage.MessageName);
			else
			{
				if(responce.OK)
				{
					Console.WriteLine("Token Validation OK");
					Console.WriteLine(responce.Responce);
					Console.WriteLine(responce.SessionID);
					LastSessionToken = responce.SessionID;
				}
				else
				{
					Console.WriteLine("Token Validation Failed");
					Console.WriteLine(responce.Responce);
				}
			}

			ClearWait();
		}

		static void ValidateAuthToken()
		{
			ValidateAuthenticationTokenRequest request = new ValidateAuthenticationTokenRequest();

			request.UserID = LastUserID;
			request.Token = LastAuthToken;
			request.APIKey = "Test Frame";

			Console.WriteLine("Auth Token Validation Sent");
			Console.WriteLine();

			SetWait();
			Client.SendMessage(request, null, ValidateAuthTokenReceived);

			while(Wait())
				Thread.Sleep(100);
		}

		private static void ValidateAuthTokenReceived(object sender, JsonClient.JsonMessageResponceArgs e)
		{
			ValidateAuthenticationTokenResponce responce = e.ResponceMessage as ValidateAuthenticationTokenResponce;
			if(responce == null)
				Console.WriteLine("Invalid responce" + e.ResponceMessage.MessageName);
			else
			{
				if(responce.OK)
				{
					Console.WriteLine("Auth Token Validation OK");
					Console.WriteLine(responce.Responce);
				}
				else
				{
					Console.WriteLine("Auth Token Validation Failed");
					Console.WriteLine(responce.Responce);
				}
			}

			ClearWait();
		}
	}
}
