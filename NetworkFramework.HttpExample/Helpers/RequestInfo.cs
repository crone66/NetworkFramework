/*
 * Author: Marcel Croonenbroeck
 * Date: 28.09.2017
 */
namespace NetworkFramework.HttpExample
{
    /// <summary>
    /// Stores a parsed http request
    /// </summary>
    public struct RequestInfo
    {
        public MethodeType Methode;
        public EncodingType Encoding;
        public string HttpVersion;
        public string Content;
        public string Host;
        public bool KeepAlive;
        public RequestInfo(MethodeType methode, string content, string host, bool keepAlive, string httpVersion, EncodingType encoding)
        {
            Methode = methode;
            HttpVersion = httpVersion;
            Content = content;
            Host = host;
            KeepAlive = keepAlive;
            Encoding = encoding;
        }
    }
}
