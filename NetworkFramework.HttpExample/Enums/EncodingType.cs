using System;

namespace NetworkFramework.HttpExample
{
    [Flags]
    public enum EncodingType
    {
        NONE,
        GZIP,
        DEFLATE,
    }
}
