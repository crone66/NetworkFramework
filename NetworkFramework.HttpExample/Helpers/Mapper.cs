using System;
using System.Collections.Generic;

namespace NetworkFramework.HttpExample
{
    public static class Mapper
    {
        private static Dictionary<string, string> fileFormatMapping = new Dictionary<string, string>()
        {
            {"default", "text/html" },
            {"html", "text/html" },
            {"htm", "text/html" },
            {"shtml", "text/html" },
            {"txt", "text/plain" },
            {"css", "text/css" },
            {"js", "text/javascript" },
            {"csv", "text/comma-separated-values" },
            {"xml", "text/xml" },

            {"png", "image/png" },
            {"jpg", "image/jpeg" },
            {"jpe", "image/jpeg" },
            {"jpeg", "image/jpeg" },
            {"gif", "image/gif" },
            {"ico", "image/x-icon" },
            {"tiff", "image/tiff" },
            {"tif", "image/tiff" },

            {"mp4", "video/mpeg" },
            {"mpeg", "video/mpeg" },
            {"mpg", "video/mpeg" },
            {"mpe", "video/mpeg" },
            {"avi", "video/x-msvideo" },
            {"qt", "video/quicktime" },
            {"mov", "video/quicktime" },
            {"mp3", "audio/mpeg" },
            {"wav", "audio/x-wav" },

            {"svg", "application/svg+xml" },
            {"pdf", "application/pdf"}
        };

        public static string GetContentType(string ext)
        {
            if (ext[0] == '.')
                ext = ext.Substring(1);

            if (fileFormatMapping.ContainsKey(ext))
                return fileFormatMapping[ext];
            else
                return fileFormatMapping["default"];
        }


        public static MethodeType GetMethodeType(string text)
        {
            if (Enum.TryParse(text, out MethodeType type))
            {
                return type;
            }
            return MethodeType.NONE;
        }

        public static string GetMethodeType(MethodeType type)
        {
            return nameof(type);
        }

        public static EncodingType GetEncoding(string text)
        {
            EncodingType res = EncodingType.NONE;
            string[] types = text.Split(',');
            foreach (string item in types)
            {
                if (Enum.TryParse(item.Trim().ToUpper(), out EncodingType type))
                {
                    res |= type;
                }
            }
            return res;
        }

        public static string GetEncoding(EncodingType type)
        {
            return nameof(type).ToLower();
        }
    }
}
