using System;
using System.Text;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Share;

namespace Server
{
    class ServerSide : SocketServer
    {
        public AsyncCallback workerCallBack;
        private Socket controlSocket;
        Dictionary<string, Socket> workerSocketList = new Dictionary<string, Socket>();

        public override void Start(string port)
        {

            try
            {
                Log("starting on port " + port);
                controlSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint ipLocal = new IPEndPoint(IPAddress.Any, Int32.Parse(port));
                controlSocket.Bind(ipLocal);
                controlSocket.Listen(4);
                controlSocket.BeginAccept(new AsyncCallback(OnClientConnect), null);
                Log("started!");

            }
            catch (SocketException se)
            {
                Log("SocketException " + se.ErrorCode + " : " + se.Message);
            }
        }

        public override void Stop()
        {
            Log("Server stopping");

            if (controlSocket != null)
            {
                controlSocket.Close();
            }

            controlSocket = null;

            foreach (KeyValuePair<string, Socket> pair in workerSocketList)
            {
                CloseClientConnection(pair.Value);
                Log("    " + pair.Key + "shut down");
            }

            workerSocketList.Clear();
            Log("server stopped!");

        }

        public override void SendToClients(string msg)
        {
            Log("broadcasting /" + msg + "/");
            foreach (KeyValuePair<string, Socket> pair in workerSocketList)
            {
                SendToClient(pair.Value, msg);
                Log("    " + pair.Key + "\"" + msg + "\"");
            }
            Log("broadcasting finished!");
        }

        public void OnClientConnect(IAsyncResult asyn)
        {

            if (controlSocket == null) { return; }

            try
            {
                Socket connectingClient = controlSocket.EndAccept(asyn);
                IPEndPoint clientEndPoint = connectingClient.RemoteEndPoint as IPEndPoint;
                workerSocketList.Add(AddLogHeader(clientEndPoint), connectingClient);
                WaitForData(connectingClient);
                Log(AddLogHeader(clientEndPoint) + "client connected!");
                controlSocket.BeginAccept(new AsyncCallback(OnClientConnect), null);
            }
            catch (SocketException se)
            {
                Log("SocketException " + se.ErrorCode + " : " + se.Message);
            }

        }

        public void WaitForData(Socket soc)
        {

            try
            {
                if (workerCallBack == null)
                {
                    workerCallBack = new AsyncCallback(OnDataReceived);
                }

                SocketPacket theSocPkt = new SocketPacket();
                theSocPkt.currentSocket = soc;
                soc.BeginReceive(theSocPkt.dataBuffer, 0, theSocPkt.dataBuffer.Length, SocketFlags.None, workerCallBack, theSocPkt);

            }
            catch (SocketException se)
            {
                Log("SocketException " + se.ErrorCode + " : " + se.Message);
            }

        }

        public void OnDataReceived(IAsyncResult asyn)
        {

            SocketPacket socketData;
            IPEndPoint client;

            if (controlSocket == null) { return; }

            try
            {
                socketData = asyn.AsyncState as SocketPacket;
                client = socketData.currentSocket.RemoteEndPoint as IPEndPoint;
                int receiveCount = socketData.currentSocket.EndReceive(asyn);
                if (receiveCount == 0)
                {
                    RemoveClient(client);
                }
                else
                {
                    string msg = Encoding.UTF8.GetString(socketData.dataBuffer, 0, receiveCount);
                    Log(AddLogHeader(client) + "\"" + msg + "\"");
                    WaitForData(socketData.currentSocket);
                }

            }
            catch (SocketException se)
            {
                if (se.ErrorCode == 10054)
                {
                    socketData = asyn.AsyncState as SocketPacket;
                    client = socketData.currentSocket.RemoteEndPoint as IPEndPoint;
                    RemoveClient(client);
                }
                else
                {
                    Log("SocketException " + se.ErrorCode + " : " + se.Message);
                }

            }

        }

        protected void RemoveClient(IPEndPoint client)
        {

            if (workerSocketList.ContainsKey(AddLogHeader(client)))
            {
                Socket clientSocket = workerSocketList[AddLogHeader(client)];
                CloseClientConnection(clientSocket);
                workerSocketList.Remove(AddLogHeader(client));
            }

            Log(AddLogHeader(client) + "client disconnected");

        }

        protected void CloseClientConnection(Socket clientSocket)
        {
            if (clientSocket != null)
            {
                clientSocket.Close();
                clientSocket = null;
            }
        }

        protected void SendToClient(Socket clientSocket, String msg)
        {

            if (clientSocket == null) { return; }

            try
            {
                byte[] dataBuffer = Encoding.UTF8.GetBytes(msg);
                clientSocket.Send(dataBuffer);
            }
            catch (SocketException se)
            {
                Log("SocketException " + se.ErrorCode + " : " + se.Message);
            }

        }
    }
}
