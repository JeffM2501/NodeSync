using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System.Threading;

namespace LobbyNode.Security
{
    public class BanList
    {
        public class BanItem
        {
            public string Identifyer = string.Empty;
            public string Notes = string.Empty;
        }
        public List<BanItem> BannedIPs = new List<BanItem>();
        public List<BanItem> BannedHosts = new List<BanItem>();

        internal Dictionary<string, BanItem> IPCache = new Dictionary<string, BanItem>();
        internal Dictionary<string, BanItem> HostCache = new Dictionary<string, BanItem>();

        public void Cache()
        {
            lock(IPCache)
            {
                IPCache.Clear();
                foreach (var item in BannedIPs)
                    IPCache.Add(item.Identifyer, item);
            }

            lock (HostCache)
            {
                HostCache.Clear();
                foreach (var item in BannedHosts)
                    HostCache.Add(item.Identifyer.ToLowerInvariant(), item);
            }
        }

        public virtual bool CheckSingleIP(string ip)
        {
            lock (IPCache)
            {
                return IPCache.ContainsKey(ip);
            }
        }

        public virtual bool CheckIPV4(string ip)
        {
            string[] parts = ip.Split(".".ToCharArray());
            for(int i = parts.Length; i > 0; i--)
            {
                string t = string.Join(".", parts, 0, i);
                if (CheckSingleIP(ip))
                    return true;
            }

            return false;
        }

        public virtual bool CheckIPV6(string ip)
        {
            string[] parts = ip.Split(":".ToCharArray());
            for (int i = parts.Length; i > 0; i--)
            {
                string t = string.Join(":", parts, 0, i);
                if (CheckSingleIP(ip))
                    return true;
            }

            return false;
        }

        public virtual bool CheckHost(string host)
        {
            lock (HostCache)
            {
                return HostCache.ContainsKey(host.ToLowerInvariant());
            }
        }

        public static BanList LoadFromXML(string path)
        {
            try
            {
                FileInfo file = new FileInfo(path);
                XmlSerializer xml = new XmlSerializer(typeof(BanList));
                StreamReader sr = file.OpenText();
                BanList b = xml.Deserialize(sr) as BanList;
                sr.Close();
                if (b == null)
                    b = new BanList();

                return b;
            }
            catch(Exception /*ex*/)
            {
                return new BanList();
            }
        }
    }

    public class BanProcessrUserArgs : EventArgs
    {
        public enum ResultTypes
        {
            Rejected,
            RejectedIP,
            RejectedHost,
            Accepted,
        }
        public ResultTypes Results = ResultTypes.Rejected;
        public LobbyUser User = null;
        public object Token = null;
    }

    public class BanProcessor
    {
        private Thread Worker = null;

        protected class BanCheckUser
        {
            public LobbyUser User = null;
            public event EventHandler<BanProcessrUserArgs> Callback = null;
			public object Token = null;

            public void CallCallback(BanProcessrUserArgs args)
            {
                Callback?.Invoke(User, args);
            }
        }
        protected List<BanCheckUser> CheckingUsers = new List<BanCheckUser>();

        public BanList Bans = null;

        public void Add(LobbyUser user, EventHandler<BanProcessrUserArgs> cb, object token)
        {
            BanCheckUser bcu = new BanCheckUser();
            bcu.User = user;
			bcu.Token = token;
            bcu.Callback += cb;

            lock (CheckingUsers)
                CheckingUsers.Add(bcu);

            CheckAlive();
        }

        protected BanCheckUser PopUser()
        {
            lock(CheckingUsers)
            {
                if (CheckingUsers.Count == 0)
                    return null;

                BanCheckUser bcu = CheckingUsers[0];
                CheckingUsers.RemoveAt(0);
                return bcu;
            }
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
            BanCheckUser bcu = PopUser();

            while (bcu != null)
            {
                BanProcessrUserArgs args = new BanProcessrUserArgs();
                args.User = bcu.User;
				args.Token = bcu.Token;

                bool ipCheckValid = false;

                if (bcu.User.SocketConnection.RemoteEndPoint.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    if (!Bans.CheckIPV4(bcu.User.SocketConnection.RemoteEndPoint.Address.ToString()))
                        ipCheckValid = true;
                }
                else if (bcu.User.SocketConnection.RemoteEndPoint.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                {
                    if (!Bans.CheckIPV6(bcu.User.SocketConnection.RemoteEndPoint.Address.ToString()))
                        ipCheckValid = true;
                }

                if (ipCheckValid)
                {
                    if (Bans.CheckHost(System.Net.Dns.GetHostEntry(bcu.User.SocketConnection.RemoteEndPoint.Address).HostName))
                        args.Results = BanProcessrUserArgs.ResultTypes.RejectedHost;
                    else
                        args.Results = BanProcessrUserArgs.ResultTypes.Accepted;
                        
                }
                else
                    args.Results = BanProcessrUserArgs.ResultTypes.RejectedIP;


                bcu.CallCallback(args);


                bcu = PopUser();
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
