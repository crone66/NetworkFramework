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
        protected bool active;
        protected int bufferLength;
        protected UdpClient client;

        protected IPEndPoint remoteEndPoint;
        protected IPEndPoint listenEndPoint;

        public event EventHandler<UDPMessageArgs> OnMessageReceived;
        public event EventHandler<ConnectionErrorArgs> OnException;

        public IPEndPoint LocalEndPoint
        {
            get { return listenEndPoint; }
        }

        public SimpleUdpClient(int bufferLength, IPEndPoint listenEndPoint, IPEndPoint remoteEndPoint)
        {
            this.bufferLength = bufferLength;
            this.listenEndPoint = listenEndPoint;
            this.remoteEndPoint = remoteEndPoint;
        }

        public SimpleUdpClient(int bufferLength, IPEndPoint listenEndPoint)
        {
            this.bufferLength = bufferLength;
            this.listenEndPoint = listenEndPoint;
        }

        /// <summary>
        /// Starts Receiver
        /// </summary>
        /// <returns></returns>
        public virtual bool Start()
        {
            if (!active)
            {
                active = true;
                try
                {
                    client = new UdpClient(listenEndPoint);
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
        public virtual void Stop()
        {
            if (active)
            {
                active = false;

                try
                {
                    client.Close();
                }
                catch
                { }
                client = null;
            }
        }

        /// <summary>
        /// Send a message to server 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public virtual async Task<bool> SendAsync(byte[] message)
        {
            return await SendAsync(message, remoteEndPoint);
        }

        /// <summary>
        /// Send a message to server 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="remoteEndPoint">Receivers IP endpoint</param>
        /// <returns></returns>
        public virtual async Task<bool> SendAsync(byte[] message, IPEndPoint remoteEndPoint)
        {
            try
            {
                if (active)
                {
                    if (message != null && message.Length > 0 && message.Length <= bufferLength)
                    {
                        if (client != null && remoteEndPoint != null)
                        {
                            return await client.SendAsync(message, message.Length, remoteEndPoint) == message.Length;
                        }
                        else
                        {
                            OnException?.Invoke(this, new ConnectionErrorArgs(new NullReferenceException("The receiver udp client or remote EndPoint is null"), true));
                        }
                    }
                    else
                    {
                        OnException?.Invoke(this, new ConnectionErrorArgs(new Exception("Invalid message! Message cannot be null and the length must be greater then zero and lower or equal to bufferLength"), true));
                    }
                }
                else
                {
                    OnException?.Invoke(this, new ConnectionErrorArgs(new Exception("The Start method has to be called before you can send messages!"), true));
                }
            }
            catch (Exception ex)
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
        public virtual async Task<bool> SendAsync(IPEndPoint remoteEndPoint, byte[] message)
        {
            try
            {
                if (client != null)
                {
                    return await client.SendAsync(message, message.Length, remoteEndPoint) == message.Length;
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
        protected virtual async void ReadAsync()
        {
            try
            {
                UdpReceiveResult result = await client.ReceiveAsync();

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

        protected virtual void InvokeOnException(ConnectionErrorArgs errorArgs)
        {
            OnException?.Invoke(this, errorArgs);
        }

        protected virtual void InvokeOnReceivedMessage(UDPMessageArgs messageArgs)
        {
            OnMessageReceived?.Invoke(this, messageArgs);
        }
    }
}
