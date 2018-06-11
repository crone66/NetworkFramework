/*
 * Author: Marcel Croonenbroeck
 * Date: 28.09.2017
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace HTTPServer
{
    /// <summary>
    /// stores http response values
    /// </summary>
    public struct ResponseInfo
    {
        public string HttpVersion;
        public string StatusCode;
        public string StatusName;
        public string LastModified;
        public string ContentLang;
        public string ContentType;
        public string ContentEncoding;
        public byte[] Content;
        public bool KeepAlive;
        public string Charset;

        public ResponseInfo(string httpVersion, string statusCode, string statusName, string lastModified, string contentLang, string contentType, byte[] content, bool keepAlive, string charset, string contentEncoding)
        {
            HttpVersion = httpVersion;
            StatusCode = statusCode;
            StatusName = statusName;
            LastModified = lastModified;
            ContentLang = contentLang;
            ContentType = contentType;
            Content = content;
            KeepAlive = keepAlive;
            Charset = charset;
            ContentEncoding = contentEncoding;
        }

        /// <summary>
        /// Creates a http response
        /// </summary>
        /// <param name="lf">Line feed character (LF=char10)</param>
        /// <returns>Http response as byte array</returns>
        public byte[] CreateResponse(char lf, int orginalLength = -1, int startIndex = -1, int endIndex = -1)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("HTTP/{0} {1} {2}{3}", HttpVersion, StatusCode, StatusName, lf));
            sb.Append(string.Format("Date: {0}{1}", DateTime.Now.ToUniversalTime(), lf));

            if(LastModified != null)
                sb.Append(string.Format("Last-Modified: {0}{1}", LastModified, lf));

            if (ContentLang != null)
                sb.Append(string.Format("Content-Language: {0}{1}", ContentLang, lf));

            sb.Append(string.Format("Connection: {0}{1}", (KeepAlive ? "keep-alive" : "close"), lf));

            if (ContentType != null)
            {
                if (Charset != null)
                    sb.Append(string.Format("Content-Type: {0}; charset={1}{2}", ContentType, Charset, lf));
                else
                {
                    sb.Append(string.Format("Content-Type: {0}{1}", ContentType, lf));
                    sb.Append(string.Format("Accept-Ranges: bytes{0}", lf));
                }
            }

            if (ContentEncoding != null)
                sb.Append(string.Format("Content-Encoding: {0}{1}", ContentEncoding, lf));

            if (Content != null)
                sb.Append(string.Format("Content-Length: {0}{1}", Content.Length, lf));

            if(startIndex > -1 || endIndex > -1)
            {
                string startValue = "";
                if (startIndex > -1)
                    startValue = startIndex.ToString();

                string endValue = "";
                if (endIndex > -1)
                    endValue = endIndex.ToString();

                string length = "";
                if (orginalLength == -1) //live stream
                    length = "*";
                else
                    length = orginalLength.ToString();

                sb.Append(string.Format("Content-Range: {0}-{1}/{2}{3}", startValue, endValue, length, lf));
            }


            sb.Append(lf);

            //Setup final byte array
            List<byte> data = new List<byte>();
            data.AddRange(Encoding.ASCII.GetBytes(sb.ToString()));
            data.AddRange(Content);

            sb.Clear();
            sb.Append(lf);
            sb.Append(lf);
            data.AddRange(Encoding.ASCII.GetBytes(sb.ToString()));
            
            return data.ToArray();
        }
    }
}
