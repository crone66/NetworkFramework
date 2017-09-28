/*
 * Author: Marcel Croonenbroeck
 * Date: 25.09.2017
 */
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

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
        public event EventHandler OnDisconnect;

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

        /// <summary>
        /// Starts connection to a remote endpoint.
        /// </summary>
        /// <param name="remoteEndPoint">Remote endpoint</param>
        /// <returns>Returns true on success</returns>
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

        /// <summary>
        /// Stop the client and closes all streams (Can cause exceptions)
        /// </summary>
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
                OnDisconnect?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Sends a message to a connected remote client
        /// </summary>
        /// <param name="message">Message as byte array</param>
        /// <returns>Returns true on success. (Indicates only that the message was successfully loaded into the network buffer!)</returns>
        public async Task<bool> SendAsync(byte[] message)
        {
            try
            {
                if(active && stream.CanWrite)
                    await stream.WriteAsync(message, 0, message.Length);

                return true;
            }
            catch(Exception ex)
            {
                OnException?.Invoke(this, new ConnectionErrorArgs(ex, false));
                return false;
            }
        }

        /// <summary>
        /// Receiver methode runs in a async thread and calls it self when a message was received
        /// </summary>
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
