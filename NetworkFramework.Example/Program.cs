using System;

namespace NetworkFramework.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            ServerTcp server = new ServerTcp();
            ClientTcp client = new ClientTcp();

            ServerUdp udpServer = new ServerUdp();
            ClientUdp udpClient = new ClientUdp();
            udpClient.Send();

            Console.ReadKey();
        }
    }
}
