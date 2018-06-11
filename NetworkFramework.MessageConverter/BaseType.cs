using System;
namespace NetworkFramework.MessageConverter
{
    [Flags]
    public enum BaseType : byte
    {
        None = 0,
        Long = 1,
        Int = 2,
        Short = 3,
        ULong = 4,
        UInt = 5,
        UShort = 6,
        Float = 7,
        Double = 8,
        Decimal = 9,
        Bool = 10,
        Char = 11,
        Byte = 12,
        String = 13,
        HasLength = 64, //flag: indicates whenether a argument has length parameter
        Array = 128, //flag: indicates array
    }
}
