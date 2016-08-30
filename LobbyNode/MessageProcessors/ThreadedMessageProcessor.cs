using System;
using System.Collections.Generic;
using System.Threading;

using NetworkingMessages.Messages;

namespace LobbyNode.MessageProcessors
{
	public class ThreadedMessageProcessor : IUserMessageProcessor
	{
		protected int MaxMessagesPerCycle = 10;
		protected int ThreadSleepTime = 25;
		protected int NoMessageSleepFactor = 10;

		private Thread Worker = null;

		private List<LobbyUser> AddedUsers = new List<LobbyUser>();
		private List<LobbyUser> RemovedUsers = new List<LobbyUser>();

		protected List<LobbyUser> ConnectedUsers = new List<LobbyUser>();

		public void PeerAdded(LobbyUser user)
		{
			lock(AddedUsers)
				AddedUsers.Add(user);

			CheckAlive();
		}

		public void PeerRemoved(LobbyUser user)
		{
			lock(RemovedUsers)
				RemovedUsers.Add(user);
		}

		public void ReceivePeerData(NetworkMessage msg, LobbyUser user)
		{
			user.PushInboundMessage(msg);
			CheckAlive();
		}

		public void Start()
		{
			Stop();
			Startup();
			CheckAlive();
		}

		public void Stop()
		{
			if(Worker != null)
				Worker.Abort();

			Worker = null;

			ConnectedUsers.Clear();

			lock(AddedUsers)
				AddedUsers.Clear();
			lock(RemovedUsers)
				RemovedUsers.Clear();

			Shutdown();
		}

		protected void CheckAlive()
		{
			if(Worker != null)
				return;
			Worker = new Thread(new ThreadStart(DoWork));
			Worker.Start();
		}

		protected virtual void DoWork()
		{
			bool done = false;

			while(!done)
			{
				List<LobbyUser> deaded = new List<LobbyUser>();
				List<LobbyUser> born = new List<LobbyUser>();

				lock(RemovedUsers)
				{
					if(RemovedUsers.Count > 0)
						deaded.AddRange(RemovedUsers.ToArray());
					RemovedUsers.Clear();
				}
				
				lock(AddedUsers)
				{
					if(AddedUsers.Count > 0)
						born.AddRange(AddedUsers.ToArray());
					AddedUsers.Clear();
				}

				foreach(var user in deaded)
				{
					ConnectedUsers.Remove(user);
					HandleUserRemoved(user);
				}

				foreach(var user in born)
				{
					if(!deaded.Contains(user))
					{
						ConnectedUsers.Add(user);
						HandleUserAdded(user);
					}
				}
				
				if (ConnectedUsers.Count ==0)
				{
					done = true;
					break;
				}

				int count = 0;
		
				bool hadMessage = true;
				bool hadOne = false;

				while(count < MaxMessagesPerCycle && hadMessage)
				{
					hadMessage = false;
					foreach(var user in ConnectedUsers)
					{
						NetworkMessage msg = user.PopInboundMessage();
						if (msg != null)
						{
							HandleUserMessage(user, msg);
							hadMessage = true;
							hadOne = true;
						}
					}
					count++;
				}
                Tick();

                Thread.Sleep(hadOne ? ThreadSleepTime : ThreadSleepTime * NoMessageSleepFactor);
			}

			Worker = null;
		}

		protected virtual void Startup()
		{

		}

		protected virtual void Shutdown()
		{

		}

		protected virtual void HandleUserRemoved(LobbyUser user)
		{

		}
		protected virtual void HandleUserAdded(LobbyUser user)
		{

		}

		protected virtual void HandleUserMessage(LobbyUser user, NetworkMessage message)
		{

		}

        protected virtual void Tick()
        {

        }
	}
}
