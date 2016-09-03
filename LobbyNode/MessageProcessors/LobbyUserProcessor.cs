using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkingMessages.Messages;

namespace LobbyNode.MessageProcessors
{
    public class LobbyUserProcessor : ThreadedMessageProcessor
    {
        public static List<LobbyUserProcessor> ChatLobbies = new List<LobbyUserProcessor>();
        public static int LastLobbyID = 0;

        private static object LobbyCheckLocker = new object();
        private static LobbyUserProcessor LastPreferedLobby = null;

        public static LobbyUserProcessor FindBestLobbyProcessor(int softLimit)
        {
            lock (LobbyCheckLocker)
            {
                if (LastPreferedLobby != null)
                {
                    if (LastPreferedLobby.UserCount() >= softLimit)
                        LastPreferedLobby = null;
                }

                if (LastPreferedLobby == null) // first time, or can't use the last one
                {
                    int smallest = int.MaxValue;
                    lock (ChatLobbies)
                    {
                        foreach (var c in ChatLobbies)
                        {
                            int count = c.UserCount();
                            if (count < softLimit)
                            {
                                if (count < smallest)
                                {
                                    smallest = count;
                                    LastPreferedLobby = c;
                                }
                            }
                        }
                    }
                }

                if (LastPreferedLobby == null)      // empty or nothing under limit, make a new one
                {
                    LastPreferedLobby = new LobbyUserProcessor();
                        LastPreferedLobby.Name = "Default (" + LastPreferedLobby.ID.ToString() + ")";
                }
            }
            return LastPreferedLobby;
        }

        public int ID = 0;
        public string Name = string.Empty;

        public LobbyUserProcessor()
        {
            lock (ChatLobbies)
            {
                LastLobbyID++;
                ID = LastLobbyID;

                ChatLobbies.Add(this);
            }
        }

        public int UserCount()
        {
            lock (ConnectedUsers)
                return ConnectedUsers.Count;
        }

        protected override void HandleUserAdded(LobbyUser user)
        {
            base.HandleUserAdded(user);
        }

        protected override void HandleUserRemoved(LobbyUser user)
        {
            base.HandleUserRemoved(user);
        }

        protected override void HandleUserMessage(LobbyUser user, NetworkMessage message)
        {
            base.HandleUserMessage(user, message);
        }
    }
}
