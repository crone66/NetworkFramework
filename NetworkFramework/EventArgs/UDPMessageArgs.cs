/*
 * Author: Marcel Croonenbroeck
 * Date: 25.09.2017
 */
using System.Net;

namespace NetworkFramework
{
    public class UDPMessageArgs : MessageArgs
    {
        public IPEndPoint Remote;

        public UDPMessageArgs(IPEndPoint remote, byte[] message) : base(message)
        {
            Remote = remote;
        }
    }
}
