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
    public class SimpleUdpClient
    {
        private bool active;
        private UdpClient receiver;

        private IPEndPoint remoteEndPoint;
        private IPEndPoint listenEndPoint;

        public event EventHandler<UDPMessageArgs> OnMessageReceived;
        public event EventHandler<ConnectionErrorArgs> OnException;

        public IPEndPoint LocalEndPoint
        { get { return listenEndPoint; } }

        public SimpleUdpClient(IPEndPoint listenEndPoint, IPEndPoint remoteEndPoint)
        {
            this.listenEndPoint = listenEndPoint;
            this.remoteEndPoint = remoteEndPoint;
        }

        public SimpleUdpClient(IPEndPoint listenEndPoint)
        {
            this.listenEndPoint = listenEndPoint;
        }

        /// <summary>
        /// Starts Receiver
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Stops Receiver
        /// </summary>
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

        /// <summary>
        /// Send a message to server 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<bool> SendAsync(byte[] message)
        {
            try
            {
                if (receiver != null && remoteEndPoint != null)
                {
                    return await receiver.SendAsync(message, message.Length, remoteEndPoint) == message.Length;
                }
            }
            catch(Exception ex)
            {
                OnException?.Invoke(this, new ConnectionErrorArgs(ex, false));
            }
            return false;
        }

        /// <summary>
        /// Sends a message to given remote endpoint
        /// </summary>
        /// <param name="remoteEndPoint"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<bool> SendAsync(IPEndPoint remoteEndPoint, byte[] message)
        {
            try
            {
                if (receiver != null)
                {
                    return await receiver.SendAsync(message, message.Length, remoteEndPoint) == message.Length;
                }
            }
            catch(Exception ex)
            {
                OnException?.Invoke(this, new ConnectionErrorArgs(ex, false));
            }
            return false;
        }

        /// <summary>
        /// Receives udp messages and calls itself again
        /// </summary>
        private async void ReadAsync()
        {
            try
            {
                UdpReceiveResult result = await receiver.ReceiveAsync();

                if (active)
                    ReadAsync();

                if (result != null)
                    OnMessageReceived?.Invoke(this, new UDPMessageArgs(result.RemoteEndPoint, result.Buffer));
            }
            catch(Exception ex)
            {
                Stop();
                OnException?.Invoke(this, new ConnectionErrorArgs(ex, true));
            }
        }
    }
}
