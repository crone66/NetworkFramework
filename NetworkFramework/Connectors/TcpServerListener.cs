/*
 * Author: Marcel Croonenbroeck
 * Date: 25.09.2017
 */
using System;
using System.Net;
using System.Net.Sockets;

namespace NetworkFramework
{
    /// <summary>
    /// Tcp listener
    /// </summary>
    public class TcpServerListener
    {
        private bool active;
        private TcpListener listener;
        private IPEndPoint localEndPoint;

        public event EventHandler<NewConnectionArgs> OnConnectionAccepted;
        public event EventHandler<ConnectionErrorArgs> OnException;

        public TcpServerListener(IPEndPoint localEndPoint)
        {
            this.localEndPoint = localEndPoint;
        }

        /// <summary>
        /// Starts listening on a given local end point
        /// </summary>
        /// <param name="localEndPoint"></param>
        /// <returns></returns>
        public bool Start(IPEndPoint localEndPoint = null)
        {
            if (!active)
            {
                try
                {
                    active = true;
                    if (localEndPoint != null)
                        this.localEndPoint = localEndPoint;

                    if (this.localEndPoint != null)
                    {
                        try
                        {
                            listener = new TcpListener(this.localEndPoint);
                            listener.Start();
                            Listen();

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
                catch
                {
                    active = false;
                    listener = null;
                }
            }

            return false;
        }

        /// <summary>
        /// Stops listening
        /// </summary>
        public void Stop()
        {
            if(active)
            {
                try
                {
                    listener.Stop();
                }
                catch
                { }
                listener = null;
                active = false;
            }
        }

        /// <summary>
        /// Listening methode calls itself when a new client was accepted
        /// </summary>
        private async void Listen()
        {
            try
            {
                TcpClient newClient = await listener.AcceptTcpClientAsync();

                if (active)
                    Listen();

                OnConnectionAccepted?.Invoke(this, new NewConnectionArgs(newClient));
            }
            catch(Exception ex)
            {
                OnException?.Invoke(this, new ConnectionErrorArgs(ex, false));

                if (active)
                    Listen();
            }
        }
    }
}
