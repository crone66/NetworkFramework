using System;

namespace NetworkFramework
{
    public class MessageArgs : EventArgs
    {
        public byte[] Message;
        public MessageArgs(byte[] message)
        {
            Message = message;
        }
    }
}
