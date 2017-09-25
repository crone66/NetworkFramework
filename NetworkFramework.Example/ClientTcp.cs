using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetworkFramework.Example
{
    public class ClientTcp
    {
        private SimpleTcpClient client;

        public ClientTcp()
        {
            client = new SimpleTcpClient(1024, new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11000));
            client.OnException += Client_OnException;
            client.OnReceivedMessage += Client_OnReceivedMessage;
            client.Start();
            client.SendAsync(Encoding.ASCII.GetBytes("Hello Im a new client"));
        }

        private void Client_OnReceivedMessage(object sender, MessageArgs e)
        {
            Console.WriteLine("[TCPClient] Received message from Server: " + Encoding.ASCII.GetString(e.Message));
        }

        private void Client_OnException(object sender, ConnectionErrorArgs e)
        {
            Console.WriteLine("[TCPClient] Error: " + e.Error.Message);
        }
    }
}
