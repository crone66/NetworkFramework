/*
 * Author: Marcel Croonenbroeck
 * Date: 26.09.2017
 */
using System;
using System.Net;
using System.Text;

namespace NetworkFramework.Example
{
    public class ClientUdp
    {
        private SimpleUdpClient client; 
        public ClientUdp()
        {
            client = new SimpleUdpClient(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0), new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11001));
            client.OnException += Client_OnException;
            client.OnMessageReceived += Client_OnMessageReceived;
            client.Start();
        }

        public async void Send(string msg)
        {
            await client.SendAsync(Encoding.ASCII.GetBytes(msg));
        }

        private void Client_OnMessageReceived(object sender, UDPMessageArgs e)
        {
            Console.WriteLine("[UDPClient] Received message from server: " + Encoding.ASCII.GetString(e.Message).Trim());
        }

        private void Client_OnException(object sender, ConnectionErrorArgs e)
        {
            Console.WriteLine("[UDPClient] Error: " + e.Error.Message);
        }
    }
}
