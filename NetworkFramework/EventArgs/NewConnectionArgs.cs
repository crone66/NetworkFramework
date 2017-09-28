/*
 * Author: Marcel Croonenbroeck
 * Date: 25.09.2017
 */
using System;
using System.Net.Sockets;

namespace NetworkFramework
{
    public class NewConnectionArgs : EventArgs
    {
        public TcpClient Client;
        public NewConnectionArgs(TcpClient client)
        {
            Client = client;
        }
    }
}
