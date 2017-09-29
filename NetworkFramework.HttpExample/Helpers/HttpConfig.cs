namespace NetworkFramework.HttpExample
{
    public struct HttpConfig
    {
        public string LocalIP;
        public int Port;
        public string NotFoundPath; //404
        public string NotAllowedPath; //405
        public string DefaultCharset;
        public string RootDir;
        public string FastCGIPath;

        public HttpConfig(string localIP, int port, string notFoundPath, string notAllowedPath, string defaultCharset, string rootDir, string fastCGIPath)
        {
            LocalIP = localIP;
            Port = port;
            NotFoundPath = notFoundPath;
            NotAllowedPath = notAllowedPath;
            DefaultCharset = defaultCharset;
            RootDir = rootDir;
            FastCGIPath = fastCGIPath;
        }
    }
}
