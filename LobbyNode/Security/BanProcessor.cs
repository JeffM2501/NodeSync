using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace LobbyNode.Security
{
    public class BanProcessor
    {
        private Thread Worker = null;
        protected List<LobbyUser> CheckingUsers = new List<LobbyUser>();

        public void Add(LobbyUser user)
        {
            lock (CheckingUsers)
                CheckingUsers.Add(user);

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
            if (Worker != null)
                Worker.Abort();

            Worker = null;

            lock(CheckingUsers)
                CheckingUsers.Clear();
            Shutdown();
        }

        protected void CheckAlive()
        {
            if (Worker != null)
                return;
            Worker = new Thread(new ThreadStart(DoWork));
            Worker.Start();
        }

        protected virtual void DoWork()
        {
            bool done = false;

            while (!done)
            {
                lock(CheckingUsers)
                {

                }
            }

            Worker = null;
        }

        protected virtual void Startup()
        {

        }

        protected virtual void Shutdown()
        {

        }
    }
}
