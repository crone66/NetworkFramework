using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace NetworkFramework.Example
{
    public class ServerTcp
    {
        private TcpServerListener listener;
        private List<SimpleTcpClient> connectedClients;

        public ServerTcp()
        {
            connectedClients = new List<SimpleTcpClient>();
            listener = new TcpServerListener(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11000));
            listener.OnConnectionAccepted += Listener_OnConnectionAccepted;
            listener.OnException += Listener_OnException;
            listener.Start();
        }

        private void Listener_OnConnectionAccepted(object sender, NewConnectionArgs e)
        {
            Console.WriteLine("[TCPServer] New client connected");
            SimpleTcpClient newClient = new SimpleTcpClient(1024, e.Client);
            newClient.OnException += NewClient_OnException;
            newClient.OnReceivedMessage += NewClient_OnReceivedMessage;
            newClient.Start();
            newClient.SendAsync(Encoding.ASCII.GetBytes("Hello I'm your server :D"));
            connectedClients.Add(newClient);
        }

        private void NewClient_OnReceivedMessage(object sender, MessageArgs e)
        {
            SimpleTcpClient senderClient = sender as SimpleTcpClient;
            Console.WriteLine("[TCPServer] New message received from: " + senderClient.Remote.Address.ToString() + ":" + senderClient.Remote.Port.ToString());
            Console.WriteLine("[TCPServer] message: " + Encoding.ASCII.GetString(e.Message));
        }

        private void Listener_OnException(object sender, ConnectionErrorArgs e)
        {
            Console.WriteLine("[TCPServer] Error: " + e.Error.Message);
        }

        private void NewClient_OnException(object sender, ConnectionErrorArgs e)
        {
            Console.WriteLine("[TCPServer] Error: " + e.Error.Message);
        }
    }
}
