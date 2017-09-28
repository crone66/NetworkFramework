/*
 * Author: Marcel Croonenbroeck
 * Date: 25.09.2017
 */
using System;

namespace NetworkFramework
{ 
    public class ConnectionErrorArgs : EventArgs
    {
        public Exception Error;
        public bool SocketClosed;

        public ConnectionErrorArgs(Exception error, bool socketClosed)
        {
            Error = error;
            SocketClosed = socketClosed;
        }
    }
}
