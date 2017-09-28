﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace NetworkFramework.HttpExample
{
    public class HttpServer
    {
        private const string notFoundPath = "/ErrorCodes/404.html";
        private const string notNotAllowedPath = "/ErrorCodes/405.html";
        private const string defaultCharset = "utf-8";
        private const string rootDir = "/root";
       

        private string rootFullPath;

        private char lf = (char)10;

        private TcpServerListener listener;
        private List<SimpleTcpClient> connectedClients;

        public HttpServer()
        {
            rootFullPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + rootDir;
            connectedClients = new List<SimpleTcpClient>();
            listener = new TcpServerListener(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 80));
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
                        content = Encoding.GetEncoding(defaultCharset).GetBytes(data);
                        encoding = null;
                        charset = defaultCharset;
                    }
                    else //byte encoding
                    {
                        if(info.Encoding.HasFlag(EncodingType.GZIP))
                        {
                            content = Compressions.GZip(File.ReadAllBytes(fullPath));
                            encoding = Mapper.GetEncoding(EncodingType.GZIP);
                        }
                        else if(info.Encoding.HasFlag(EncodingType.DEFLATE))
                        {
                            content = Compressions.Deflate(File.ReadAllBytes(fullPath));
                            encoding = Mapper.GetEncoding(EncodingType.DEFLATE);
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

        private ResponseInfo GetErrorResponse(int code)
        {
            if (code == 405)
            {
                byte[] data = null;
                if (File.Exists(rootFullPath + notNotAllowedPath))
                {
                    string text = File.ReadAllText(rootFullPath + notNotAllowedPath);
                    data = Encoding.UTF8.GetBytes(text);
                }
                return new ResponseInfo("1.1", "405", "Method Not Allowed", null, "de", "text/html", data, false, "utf-8", null);
            }
            else if (code == 404)
            {
                byte[] data = null;
                DateTime modifyDate = DateTime.Now;
                if (File.Exists(rootFullPath + notFoundPath))
                {
                    string text = File.ReadAllText(rootFullPath + notFoundPath);
                    data = Encoding.UTF8.GetBytes(text);
                    modifyDate = File.GetLastWriteTime(rootFullPath + notFoundPath);
                }

                return new ResponseInfo("1.1", "404", "Not Found", modifyDate.ToUniversalTime().ToString(), "de", "text/html", data, false, "utf-8", null);
            }
            else
                throw new NotImplementedException();          
        }

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
            Console.WriteLine("[TCPServer] client disconnected.");
        }
    }
}