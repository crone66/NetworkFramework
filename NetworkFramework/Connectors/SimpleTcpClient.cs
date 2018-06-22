/*
 * Author: Marcel Croonenbroeck
 * Date: 25.09.2017
 */
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NetworkFramework
{
    public class SimpleTcpClient
    {
        protected bool active;
        protected TcpClient client;
        protected NetworkStream stream;
        protected int bufferLength;
        protected IPEndPoint remoteEndPoint;

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
        public virtual bool Start(IPEndPoint remoteEndPoint = null)
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
        public virtual void Stop()
        {
            try
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
            catch
            {
                //Ignore errors while closing broken streams
            }
        }

        /// <summary>
        /// Sends a message to a connected remote client
        /// </summary>
        /// <param name="message">Message as byte array</param>
        /// <returns>Returns true on success. (Indicates only that the message was successfully loaded into the network buffer!)</returns>
        public virtual async Task<bool> SendAsync(byte[] message)
        {
            try
            {
                if (active)
                {
                    if (message != null && message.Length > 0 && message.Length <= bufferLength)
                    {
                        if (active && stream.CanWrite)
                        {
                            await stream.WriteAsync(message, 0, message.Length);
                            return true;
                        }
                        else
                        {
                            OnException?.Invoke(this, new ConnectionErrorArgs(new Exception("Socked cannot write for unknown reason!"), true));
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
                Stop();
                OnException?.Invoke(this, new ConnectionErrorArgs(ex, ex is IOException));
            }
            return false;
        }

        /// <summary>
        /// Receiver methode runs in a async thread and calls it self when a message was received
        /// </summary>
        protected virtual async void ReceiveAsync()
        {
            try
            {
                byte[] buffer = new byte[bufferLength];
                int receivedBytes = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (receivedBytes > 0)
                {
                    if(active)
                        ReceiveAsync();

                    //Remove all unused indices;
                    byte[] data = new byte[receivedBytes];
                    Array.Copy(buffer, data, receivedBytes);

                    OnReceivedMessage?.Invoke(this, new MessageArgs(data));
                }
                else
                {
                    Stop();
                }
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

        protected virtual void InvokeOnDisconnect()
        {
            OnDisconnect?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void InvokeOnReceivedMessage(MessageArgs messageArgs)
        {
            OnReceivedMessage?.Invoke(this, messageArgs);
        }
    }
}
