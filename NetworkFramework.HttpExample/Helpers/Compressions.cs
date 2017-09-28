using System.IO;
using System.IO.Compression;

namespace NetworkFramework.HttpExample
{
    public static class Compressions
    {
        public static byte[] GZip(byte[] data)
        {
            using (MemoryStream output = new MemoryStream())
            {
                using (GZipStream gzip = new GZipStream(output, CompressionMode.Compress, false))
                {
                    gzip.Write(data, 0, data.Length);
                }
                return output.ToArray();
            }
        }

        public static byte[] Deflate(byte[] data)
        {
            using (MemoryStream output = new MemoryStream())
            {
                using (DeflateStream deflate = new DeflateStream(output, CompressionMode.Compress))
                {
                    deflate.Write(data, 0, data.Length);
                }

                return output.ToArray();
            }
        }
    }
}
