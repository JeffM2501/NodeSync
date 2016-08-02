using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Listener
{
	public class PersistentListener : Host
	{
		protected Thread WorkerThread = null;

		/// <summary>
		/// Starts a listener that will restart as needed
		/// </summary>
		/// <param name="port">port to listen on</param>
		/// <param name="handler">default peer handler, must be thread safe</param>
		public PersistentListener(int port, PeerHandler handler)
		{
			DefaultPeerHandler = handler;

			Listen(port);

			WorkerThread = new Thread(new ThreadStart(DoWork));
			WorkerThread.Start();
		}

		public override void Shutdown()
		{
			// stop accepting new messages
			ShutdownWorker();
			Thread.Sleep(100);

			if(WorkerThread != null)
				WorkerThread.Abort();

			WorkerThread = null;

			lock(ConnectedPeers)
			{
				foreach(var p in ConnectedPeers)
				{
					p.Value.SocketConnection.Disconnect("shutdown");
					p.Value.SocketConnection.Peer.FlushSendQueue();
					p.Value.Handler.PeerDisconnected("Force Shutdown", p.Value);
				}
				ConnectedPeers.Clear();
			}
			base.Shutdown();
		}

		private object ExitLocker = new object();
		private bool Done = false;

		protected bool IsDone()
		{
			lock(ExitLocker)
				return Done;
		}

		protected void ShutdownWorker()
		{
			lock(ExitLocker)
				Done = true;
		}

		protected virtual void DoWork()
		{
			while (!IsDone())
			{
				try
				{
					if(!SocketIsLive())
						Listen(ActualPort);

					ProcessSockets();
				}
				catch(Exception /*ex*/)
				{

				}
				Thread.Sleep(30);
			}

			WorkerThread = null;
		}
	}
}
