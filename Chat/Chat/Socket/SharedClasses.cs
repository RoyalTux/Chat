using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace Share
{
    public class SocketPacket
    {
        public Socket currentSocket;
        public byte[] dataBuffer = new byte[1024];
    }

    public delegate void LogUIDelegate(string s);

    public class SocketCommon
    {

        public LogUIDelegate logFunction;
        protected readonly SynchronizationContext syncContext = SynchronizationContext.Current;

        protected virtual void Log(string s)
        {
            if (logFunction != null)
            {
                if (syncContext == SynchronizationContext.Current)
                {
                    logFunction(s);
                }
                else
                {
                    syncContext.Post(o => logFunction(s), null);
                }
            }
        }

        protected virtual string AddLogHeader(IPEndPoint socket)
        {
            return "/" + socket.Address.ToString() + ":" + socket.Port.ToString() + "/ ";
        }
    }

    public abstract class SocketServer : SocketCommon
    {
        public abstract void Start(string port);
        public abstract void Stop();
        public abstract void SendToClients(string msg);
    }

    public abstract class SocketClient : SocketCommon
    {
        public abstract bool Connect(string server, string port);
        public abstract void Disconnect();
        public abstract void Send(string msg);
    }

}
