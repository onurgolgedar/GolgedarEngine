using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;

namespace GolgedarEngine
{
    public class User
    {
        private static int lastConnectionID = 0;

        public User(TcpClient tcpClient) : base()
        {
            TCPClient = tcpClient;
            ConnectionID = ++lastConnectionID;
            IP = (TCPClient.Client.RemoteEndPoint as IPEndPoint).Address.ToString();
            ConnectedObjects = [];
        }

        public void Disconnect()
        {
            ConnectedObjects.ForEach(i => i.Game?.RoomData?.RemoveInstance(i));
            ConnectedObjects.Clear();

            Server.Send(this, 'D', int.MaxValue);
            TCPClient.Dispose();
        }

        public override string ToString()
        {
            return $"C[{ConnectionID}]({IP})";
        }

        public TcpClient TCPClient { get; private set; }
        public Server Server { get; private set; }
        public string IP { get; private set; }
        public int ConnectionID { get; private set; }
        public List<ConnectedGameObject> ConnectedObjects { get; protected set; }
    }
}
