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
        protected UdpClient receiver;

        protected IPEndPoint remoteEndPoint;
        protected IPEndPoint listenEndPoint;

        public event EventHandler<UDPMessageArgs> OnMessageReceived;
        public event EventHandler<ConnectionErrorArgs> OnException;

        public IPEndPoint LocalEndPoint
        {
            get { return listenEndPoint; }
        }

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
        public virtual bool Start()
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
        public virtual void Stop()
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
        public virtual async Task<bool> SendAsync(byte[] message)
        {
            try
            {
                if (active)
                {
                    if (message != null && message.Length > 0)
                    {
                        if (receiver != null && remoteEndPoint != null)
                        {
                            return await receiver.SendAsync(message, message.Length, remoteEndPoint) == message.Length;
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
        public virtual async Task<bool> SendAsync(IPEndPoint remoteEndPoint, byte[] message)
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
        protected virtual async void ReadAsync()
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
