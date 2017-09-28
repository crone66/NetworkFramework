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

        public HttpConfig(string localIP, int port, string notFoundPath, string notAllowedPath, string defaultCharset, string rootDir)
        {
            LocalIP = localIP;
            Port = port;
            NotFoundPath = notFoundPath;
            NotAllowedPath = notAllowedPath;
            DefaultCharset = defaultCharset;
            RootDir = rootDir;
        }
    }
}
