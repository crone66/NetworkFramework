/*
 * Author: Marcel Croonenbroeck
 * Date: 28.09.2017
 * Purpose: Http server example with support of all common file types (including application, audio, video, image, script and text file types) 
 */

using System;
using System.Net;
using System.Text;

namespace NetworkFramework.HttpExample
{
    class Program
    {
        static void Main(string[] args)
        {
            HttpServer srv = new HttpServer();


            // Can be used to create test requests
            /* SimpleTcpClient client = new SimpleTcpClient(2048, new IPEndPoint(IPAddress.Parse("127.0.0.1"), 80));
            client.OnReceivedMessage += Client_OnReceivedMessage;
            client.Start();
            client.SendAsync(Encoding.ASCII.GetBytes("GET /test.png HTTP/1.1" + ((char)10) +
                "HOST: localhost" + ((char)10) +
                "Range: bytes=5-" +
                ""));*/

            Console.ReadKey();
        }

        /*private static void Client_OnReceivedMessage(object sender, MessageArgs e)
        {
            Console.WriteLine(Encoding.ASCII.GetString(e.Message));
        }*/
    }
}
