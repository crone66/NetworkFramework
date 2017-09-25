using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NetworkFramework
{
    public class SimpleUdpClient
    {
        private bool active;
        private UdpClient receiver;

        private IPEndPoint remoteEndPoint;
        private IPEndPoint listenEndPoint;

        public event EventHandler<UDPMessageArgs> OnMessageReceived;
        public event EventHandler<ConnectionErrorArgs> OnException;

        public SimpleUdpClient(IPEndPoint listenEndPoint, IPEndPoint remoteEndPoint)
        {
            this.listenEndPoint = listenEndPoint;
            this.remoteEndPoint = remoteEndPoint;
        }

        public SimpleUdpClient(IPEndPoint listenEndPoint)
        {
            this.listenEndPoint = listenEndPoint;
        }

        public bool Start()
        {
            if (!active)
            {
                active = true;
                try
                {
                    receiver = new UdpClient(listenEndPoint);
                    ReadAsync();

                    return true;
                }
                catch(Exception ex)
                {
                    Stop();
                    OnException?.Invoke(this, new ConnectionErrorArgs(ex, true));
                }
            }

            return false;
        }

        public void Stop()
        {
            if (active)
            {
                active = false;

                try
                {
                    receiver.Close();
                }
                catch
                { }
                receiver = null;
            }
        }

        public async Task<bool> SendAsync(byte[] message)
        {
            if(receiver != null && remoteEndPoint != null)
            {
                return await receiver.SendAsync( message, message.Length, remoteEndPoint) == message.Length;
            }

            return false;
        }

        public async Task<bool> SendAsync(IPEndPoint remoteEndPoint, byte[] message)
        {
            if (receiver != null)
            {
                return await receiver.SendAsync(message, message.Length, remoteEndPoint) == message.Length;
            }

            return false;
        }

        private async void ReadAsync()
        {
            UdpReceiveResult result = await receiver.ReceiveAsync();

            if(active)
                ReadAsync();

            if (result != null)
                OnMessageReceived?.Invoke(this, new UDPMessageArgs(result.RemoteEndPoint, result.Buffer));
        }
    }
}
