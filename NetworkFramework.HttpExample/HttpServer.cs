/*
 * Author: Marcel Croonenbroeck
 * Date: 28.09.2017
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Xml.Serialization;

namespace NetworkFramework.HttpExample
{
    /// <summary>
    /// Http server example with support of all common file types (including application, audio, video, image, script and text file types) 
    /// </summary>
    public class HttpServer
    {
        private const string configFile = "httpConfig.xml";
        private HttpConfig config;
        private string rootFullPath;

        private char lf = (char)10;

        private TcpServerListener listener;
        private List<SimpleTcpClient> connectedClients;

        public HttpServer()
        {
            try
            {
                config = ReadConfig(configFile);
            }
            catch
            {
                config = new HttpConfig("127.0.0.1", 80, "/ErrorCodes/404.html", "/ErrorCodes/405.html", "utf-8", "/root");
                WriteConfig(configFile, config);
            }

            rootFullPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + config.RootDir;
            connectedClients = new List<SimpleTcpClient>();

            listener = new TcpServerListener(new IPEndPoint(IPAddress.Parse(config.LocalIP), config.Port));
            listener.OnConnectionAccepted += Listener_OnConnectionAccepted;
            listener.OnException += Listener_OnException;
            listener.Start();
        }

        private void Listener_OnConnectionAccepted(object sender, NewConnectionArgs e)
        {
            Console.WriteLine("[TCPServer] New client connected");
            SimpleTcpClient newClient = new SimpleTcpClient(1024, e.Client);
            newClient.OnException += NewClient_OnException;
            newClient.OnReceivedMessage += NewClient_OnReceivedMessage;
            newClient.OnDisconnect += NewClient_OnDisconnect;
            newClient.Start();
            connectedClients.Add(newClient);
        }

        private void NewClient_OnReceivedMessage(object sender, MessageArgs e)
        {
            SimpleTcpClient senderClient = sender as SimpleTcpClient;
            
            RequestInfo info = ParseRequest(Encoding.ASCII.GetString(e.Message));
            HandleRequest(senderClient, info);

            Console.WriteLine("[TCPServer] New message received from: " + senderClient.Remote.Address.ToString() + ":" + senderClient.Remote.Port.ToString());
            Console.WriteLine("[TCPServer] message: " + Encoding.ASCII.GetString(e.Message));
        }

        /// <summary>
        /// Handles http requests and sends a response
        /// </summary>
        /// <param name="client">Tcp client</param>
        /// <param name="info">Request info</param>
        private async void HandleRequest(SimpleTcpClient client, RequestInfo info)
        {
            if(info.Methode == MethodeType.GET)
            {
                string fullPath = rootFullPath + info.Content;
                if (File.Exists(fullPath))
                {
                    string type = Mapper.GetContentType(Path.GetExtension(fullPath));
                    DateTime modifiedTime = File.GetLastWriteTime(fullPath);

                    string encoding = null;
                    string charset = null;
                    byte[] content = null;
                    if (type.StartsWith("text")) //text encoding
                    {
                        string data = File.ReadAllText(fullPath);
                        content = Encoding.GetEncoding(config.DefaultCharset).GetBytes(data);
                        encoding = null;
                        charset = config.DefaultCharset;
                    }
                    else //byte encoding
                    {
                        if(info.Encoding.HasFlag(EncodingType.GZIP))
                        {
                            content = Compressions.GZip(File.ReadAllBytes(fullPath));
                            encoding = "gzip";
                        }
                        else if(info.Encoding.HasFlag(EncodingType.DEFLATE))
                        {
                            content = Compressions.Deflate(File.ReadAllBytes(fullPath));
                            encoding = "deflate";
                        }
                        else
                        {
                            content = File.ReadAllBytes(fullPath);
                        }
                    }

                    ResponseInfo response = new ResponseInfo("1.1", "200", "OK", modifiedTime.ToString(), null, type, content, info.KeepAlive, charset, encoding);
                    await client.SendAsync(response.CreateResponse(lf));
                }
                else
                {
                    await client.SendAsync(GetErrorResponse(404).CreateResponse(lf)); //file not found (404)
                }
            }
            else
            {
                await client.SendAsync(GetErrorResponse(405).CreateResponse(lf)); //Unsupported methode
            }

            if(!info.KeepAlive)
                client.Stop();
        }

        /// <summary>
        /// Predefined error code response for code 404 and 405
        /// </summary>
        /// <param name="code">Error code</param>
        /// <returns></returns>
        private ResponseInfo GetErrorResponse(int code)
        {
            if (code == 405)
            {
                byte[] data = null;
                if (File.Exists(rootFullPath + config.NotAllowedPath))
                {
                    string text = File.ReadAllText(rootFullPath + config.NotAllowedPath);
                    data = Encoding.UTF8.GetBytes(text);
                }
                return new ResponseInfo("1.1", "405", "Method Not Allowed", null, "de", "text/html", data, false, config.DefaultCharset, null);
            }
            else if (code == 404)
            {
                byte[] data = null;
                DateTime modifyDate = DateTime.Now;
                if (File.Exists(rootFullPath + config.NotFoundPath))
                {
                    string text = File.ReadAllText(rootFullPath + config.NotFoundPath);
                    data = Encoding.UTF8.GetBytes(text);
                    modifyDate = File.GetLastWriteTime(rootFullPath + config.NotFoundPath);
                }

                return new ResponseInfo("1.1", "404", "Not Found", modifyDate.ToUniversalTime().ToString(), "de", "text/html", data, false, config.DefaultCharset, null);
            }
            else
                throw new NotImplementedException("Unknown error code!");          
        }

        #region RequestParsing
        /// <summary>
        /// Parses http requests
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private RequestInfo ParseRequest(string request)
        {
            string[] lines = request.Split('\n');
            RequestInfo info = GetMethode(lines[0]);
            if (ParseVars(lines, "Host", out string host))
                info.Host = host;

            if (ParseVars(lines, "Connection", out string connection))
                info.KeepAlive = connection != "close";

            if (ParseVars(lines, "HTTP/", out string version))
                info.HttpVersion = version;

            if(ParseVars(lines, "Accept-Encoding", out string text))
            {
                info.Encoding = Mapper.GetEncoding(text);
            }


            return info;
        }

        /// <summary>
        /// Prases methode of http requests
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private RequestInfo GetMethode(string line)
        {
            string[] words = line.Split(' ');
            if (words.Length > 1)
            {
                string methode = words[0];
                string content = "";
                if(words[1].StartsWith("/"))
                    content = words[1];

                string version = null;
                ParseVars(words, "HTTP", out version);

                return new RequestInfo(Mapper.GetMethodeType(methode), content, null, true, version, EncodingType.NONE);
            }

            return default(RequestInfo); 
        }

        /// <summary>
        /// Universal parsing function for http header fields
        /// </summary>
        /// <param name="lines">Request lines</param>
        /// <param name="varName">Field name</param>
        /// <param name="value">result value</param>
        /// <returns></returns>
        private bool ParseVars(string[] lines, string varName, out string value)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                if(lines[i].StartsWith(varName))
                {
                    value = lines[i].Substring(varName.Length + 1).Trim();
                    return true;
                }
            }

            value = null;
            return false;
        }
        #endregion

        #region ConsoleOutput
        private void Listener_OnException(object sender, ConnectionErrorArgs e)
        {
            Console.WriteLine("[TCPServer] Error: " + e.Error.Message);
            Console.WriteLine(e.Error.StackTrace);
        }

        private void NewClient_OnException(object sender, ConnectionErrorArgs e)
        {
            if (!(e.Error is ObjectDisposedException))
            {
                Console.WriteLine("[TCPServer] Error: " + e.Error.Message);
                Console.WriteLine(e.Error.StackTrace);
            }
        }

        private void NewClient_OnDisconnect(object sender, EventArgs e)
        {
            if(sender != null && sender is SimpleTcpClient && connectedClients != null)
                connectedClients.Remove(sender as SimpleTcpClient);

            Console.WriteLine("[TCPServer] client disconnected.");
        }
        #endregion

        private HttpConfig ReadConfig(string path)
        {
            XmlSerializer xml = new XmlSerializer(typeof(HttpConfig));
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                return (HttpConfig)xml.Deserialize(fs);
            }
        }

        private void WriteConfig(string path, HttpConfig config)
        {
            XmlSerializer xml = new XmlSerializer(typeof(HttpConfig));
            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                xml.Serialize(fs, config);
            }
        }
    }
}
