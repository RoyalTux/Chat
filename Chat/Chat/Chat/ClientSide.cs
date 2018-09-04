using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Share;

namespace Chat
{
    class ClientSide
    {
        public delegate void DisconnectUIDelegate();

        public class SocketsClient : SocketClient
        {
            public AsyncCallback clientCallBack;
            public Socket clientSocket;
            public DisconnectUIDelegate updateScreenAfterDisconnect;

            public override bool Connect(string server, string port)
            {
                try
                {
                    Log("connecting on /" + server + ":" + port + "/");
                    clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    IPAddress ip = IPAddress.Parse(server);
                    IPEndPoint ipEnd = new IPEndPoint(ip, int.Parse(port));
                    clientSocket.Connect(ipEnd);
                    if (clientSocket.Connected)
                    {
                        Log(AddLogHeader(clientSocket.RemoteEndPoint as IPEndPoint) + "connected!");
                        WaitForData();
                    }
                    return true;
                }
                catch (SocketException se)
                {
                    Log("SocketException " + se.ErrorCode + " : " + se.Message);
                }

                return false;

            }

            public override void Disconnect()
            {
                if (clientSocket == null) { return; }

                Log(AddLogHeader(clientSocket.RemoteEndPoint as IPEndPoint) + "disconnecting");
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
                clientSocket = null;
                Log("disconnected!");
            }

            public override void Send(string msg)
            {

                if (clientSocket == null) { return; }

                try
                {

                    Log(AddLogHeader(clientSocket.RemoteEndPoint as IPEndPoint) + "send - \"" + msg + "\"");
                    byte[] dataBuffer = Encoding.UTF8.GetBytes(msg);
                    clientSocket.Send(dataBuffer);

                }
                catch (SocketException se)
                {
                    Log("SocketException " + se.ErrorCode + " : " + se.Message);
                }

            }

            public void WaitForData()
            {

                try
                {
                    if (clientCallBack == null)
                    {
                        clientCallBack = new AsyncCallback(OnDataReceived);
                    }
                    SocketPacket theSocPkt = new SocketPacket();
                    theSocPkt.currentSocket = clientSocket;
                    clientSocket.BeginReceive(theSocPkt.dataBuffer, 0, theSocPkt.dataBuffer.Length, SocketFlags.None, clientCallBack, theSocPkt);
                }
                catch (SocketException se)
                {
                    Log("SocketException " + se.ErrorCode + " : " + se.Message);
                }

            }

            public void OnDataReceived(IAsyncResult asyn)
            {

                SocketPacket socketData;
                if (clientSocket == null) { return; }

                try
                {

                    socketData = asyn.AsyncState as SocketPacket;
                    int receiveCount = socketData.currentSocket.EndReceive(asyn);

                    if (receiveCount == 0)
                    {
                        Disconnected();
                    }
                    else
                    {
                        string msg = Encoding.UTF8.GetString(socketData.dataBuffer, 0, receiveCount);
                        Log(AddLogHeader(clientSocket.RemoteEndPoint as IPEndPoint) + "receive - \"" + msg + "\"");
                        WaitForData();
                    }

                }
                catch (SocketException se)
                {
                    if (se.ErrorCode == 10054)
                    {
                        Disconnected();
                    }
                    else
                    {
                        Log("SocketException " + se.ErrorCode + " : " + se.Message);
                    }

                }

            }

            protected void Disconnected()
            {

                clientSocket = null;

                if (updateScreenAfterDisconnect != null)
                {

                    if (syncContext == SynchronizationContext.Current)
                    {
                        updateScreenAfterDisconnect();
                    }
                    else
                    {
                        syncContext.Post(o => updateScreenAfterDisconnect(), null);
                    }

                }

                Log("server disconnected!");

            }

        }
    }
}
