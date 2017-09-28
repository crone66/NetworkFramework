using System;

namespace NetworkFramework.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            ServerTcp server = new ServerTcp();
            ClientTcp client = new ClientTcp();
            client.Send("hello server im a new tcp client");

            ServerUdp udpServer = new ServerUdp();
            ClientUdp udpClient = new ClientUdp();
            udpClient.Send("hello server im a new udp client");

            Console.ReadKey();
        }
    }
}
