using System;
using System.Net;
using System.Text;

namespace NetworkFramework.Example
{
    public class ServerUdp
    {
        private SimpleUdpClient server;
        public ServerUdp()
        {
            server = new SimpleUdpClient(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11001));
            server.OnMessageReceived += Server_OnMessageReceived;
            server.OnException += Server_OnException;
            server.Start();
        }

        private void Server_OnException(object sender, ConnectionErrorArgs e)
        {
            Console.WriteLine("Error: " + e.Error.Message);
        }

        private async void Server_OnMessageReceived(object sender, UDPMessageArgs e)
        {
            Console.WriteLine("[UDPServer] Message Received from: " + e.Remote.Address.ToString() + ":" + e.Remote.Port.ToString());
            Console.WriteLine("[UDPServer] Message: " + Encoding.ASCII.GetString(e.Message));

            await server.SendAsync(e.Remote, Encoding.ASCII.GetBytes("this is my server response"));
        }
    }
}
