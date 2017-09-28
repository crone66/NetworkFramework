/*
 * Author: Marcel Croonenbroeck
 * Date: 28.09.2017
 */
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
