/*
 * Author: Marcel Croonenbroeck
 * Date: 28.09.2017
 * Purpose: Http server example with support of all common file types (including application, audio, video, image, script and text file types) 
 */

using System;

namespace NetworkFramework.HttpExample
{
    class Program
    {
        static void Main(string[] args)
        {
            HttpServer srv = new HttpServer();
            Console.ReadKey();
        }
    }
}
