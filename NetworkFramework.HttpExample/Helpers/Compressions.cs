/*
 * Author: Marcel Croonenbroeck
 * Date: 28.09.2017
 */
using System.IO;
using System.IO.Compression;

namespace NetworkFramework.HttpExample
{
    public static class Compressions
    {
        /// <summary>
        /// Gzip compression
        /// </summary>
        /// <param name="data"></param>
        /// <returns>compressed data</returns>
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

        /// <summary>
        /// Deflate compression
        /// </summary>
        /// <param name="data"></param>
        /// <returns>compressed data</returns>
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
