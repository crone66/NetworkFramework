using System;
using System.Net;
using System.Net.Sockets;

namespace NetworkFramework
{
    public class SimpleTcpClient
    {
        private bool active;
        private TcpClient client;
        private NetworkStream stream;
        private int bufferLength;
        private IPEndPoint remoteEndPoint;

        public event EventHandler<MessageArgs> OnReceivedMessage;
        public event EventHandler<ConnectionErrorArgs> OnException;

        public IPEndPoint Remote
        {
            get => (IPEndPoint)client.Client.RemoteEndPoint;
        }

        public SimpleTcpClient(int bufferLength)
        {
            this.bufferLength = bufferLength;
        }

        public SimpleTcpClient(int bufferLength, IPEndPoint remoteEndPoint)
        {
            this.bufferLength = bufferLength;
            this.remoteEndPoint = remoteEndPoint;
        }

        public SimpleTcpClient(int bufferLength, TcpClient client)
        {
            this.bufferLength = bufferLength;
            this.client = client;
        }

        public bool Start(IPEndPoint remoteEndPoint = null)
        {
            if (!active)
            {
                active = true;
                if (remoteEndPoint != null)
                    this.remoteEndPoint = remoteEndPoint;

                if (this.remoteEndPoint != null || client != null)
                {
                    try
                    {
                        if (client == null)
                        {
                            client = new TcpClient();
                            client.Connect(this.remoteEndPoint);
                        }
                        stream = client.GetStream();

                        ReceiveAsync();
                        return true;
                    }
                    catch(Exception ex)
                    {
                        Stop();
                        OnException?.Invoke(this, new ConnectionErrorArgs(ex, true));
                    }
                }
                else           
                {
                    active = false;
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
                    client.Close();
                    stream.Close();
                }
                catch
                {
                }
                client = null;
                stream = null;
            }
        }

        public async void SendAsync(byte[] message)
        {
            try
            {
                if(active && stream.CanWrite)
                    await stream.WriteAsync(message, 0, message.Length);
            }
            catch(Exception ex)
            {
                OnException?.Invoke(this, new ConnectionErrorArgs(ex, false));
            }
        }

        private async void ReceiveAsync()
        {
            try
            {
                byte[] buffer = new byte[bufferLength];
                int receivedBytes = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (receivedBytes > 0)
                {
                    if(active)
                        ReceiveAsync();

                    OnReceivedMessage?.Invoke(this, new MessageArgs(buffer));
                }
                else
                {
                    Stop();
                }
            }
            catch(Exception ex)
            {
                OnException?.Invoke(this, new ConnectionErrorArgs(ex, false));
            }
        }
    }
}
