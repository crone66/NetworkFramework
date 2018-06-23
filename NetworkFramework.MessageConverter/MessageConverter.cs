/* 
 * Author: Marcel Croonenbroeck
 * Date: 11.10.2017
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkFramework.MessageConverter
{
    public static class MessageConverter
    {
        //Cmd rule: args count, Arguments
        //Arguments Ex.:
        //baseType, value;
        //baseType(Array), count(int), value, value, ...
        //baseType(String), length(int), value
        //baseType(string,Array), count(int), length(int), value, length(int) value, ...
        
        //EX minimum cmd length: 4+1+1(int, byte, byte) = 6 bytes    
        //Ex.:       ArgCount(int)  Type(BaseType)  Value
        //Ex. Value: 1(int)         1(BaseType.Int) 1337(int)
        //Ex. Size:  4(byte)        1(byte)         4(byte)

        public static int EndcodingCodePage = 65001; //Default: Unicode (UTF-8)


        private static Dictionary<BaseType, int> sizes = new Dictionary<BaseType, int>()
        {
            { BaseType.Long, 8 },
            { BaseType.Int, 4 },
            { BaseType.Short, 2 },
            { BaseType.ULong, 8 },
            { BaseType.UInt, 4 },
            { BaseType.UShort, 2 },
            { BaseType.Float, 4 },
            { BaseType.Double, 8 },
            { BaseType.Decimal, 16 },
            { BaseType.Bool, 1 },
            { BaseType.Char, 2 },
            { BaseType.Byte, 1 },
            { BaseType.String, 0 },
        };

        private static bool ConvertArgumentToByte(object value, out byte[] data)
        {
            Type type = value.GetType();
            if(type.IsArray)
            {
                Array arr = (Array)value;
                List<byte> arrayData = new List<byte>();
                for (int i = 0; i < arr.Length; i++)
                {
                    if (ConvertArgumentToByte(arr.GetValue(i), out byte[] arrData, true, i == 0, arr.Length))
                    {
                        if(arrData != null)
                            arrayData.AddRange(arrData);
                    }
                }
                data = arrayData.ToArray();

                return data.Length > 0;
            }
            else
            {
                return ConvertArgumentToByte(value, out data, false) && data != null && data.Length > 0;
            }
        }

        private static BaseType GetBaseType(Type type)
        {
            BaseType baseType = BaseType.None;

            if (type == typeof(long))
            {
                baseType = BaseType.Long;
            }
            else if (type == typeof(int))
            {
                baseType = BaseType.Int;
            }
            else if (type == typeof(short))
            {
                baseType = BaseType.Short;
            }
            else if (type == typeof(ulong))
            {
                baseType = BaseType.ULong;
            }
            else if (type == typeof(uint))
            {
                baseType = BaseType.UInt;
            }
            else if (type == typeof(ushort))
            {
                baseType = BaseType.UShort;
            }
            else if (type == typeof(float))
            {
                baseType = BaseType.Float;
            }
            else if (type == typeof(double))
            {
                baseType = BaseType.Double;
            }
            else if (type == typeof(decimal))
            {
                baseType = BaseType.Decimal;
            }
            else if (type == typeof(bool))
            {
                baseType = BaseType.Bool;
            }
            else if (type == typeof(char))
            {
                baseType = BaseType.Char;
            }
            else if (type == typeof(byte))
            {
                baseType = BaseType.Byte;
            }
            else if (type == typeof(string))
            {
                baseType = BaseType.String | BaseType.HasLength;
            }

            if (type.IsArray)
                baseType |= BaseType.Array;

            return baseType;
        }


        public static Type GetType(BaseType baseType)
        {
            switch (baseType)
            {
                case BaseType.None:
                    return null;
                case BaseType.Long:
                    return typeof(long);
                case BaseType.Int:
                    return typeof(int);
                case BaseType.Short:
                    return typeof(short);
                case BaseType.ULong:
                    return typeof(ulong);
                case BaseType.UInt:
                    return typeof(uint);
                case BaseType.UShort:
                    return typeof(ushort);
                case BaseType.Float:
                    return typeof(float);
                case BaseType.Double:
                    return typeof(double);
                case BaseType.Decimal:
                    return typeof(decimal);
                case BaseType.Bool:
                    return typeof(bool);
                case BaseType.Char:
                    return typeof(char);
                case BaseType.Byte:
                    return typeof(byte);
                case BaseType.String:
                    return typeof(string);
            }

            return null;
        }

        private static bool ConvertArgumentToByte(object value, out byte[] data, bool isArray = false, bool isFirstArrayElement = false, int arrayLength = 0)
        {
            Type type = value.GetType();
            BaseType baseType = GetBaseType(type);

            data = null;
            switch (baseType)
            {
                case BaseType.None:
                    data = null;
                    return false;
                case BaseType.Long:
                    data = BitConverter.GetBytes((long)value);
                    break;
                case BaseType.Int:
                    data = BitConverter.GetBytes((int)value);
                    break;
                case BaseType.Short:
                    data = BitConverter.GetBytes((short)value);
                    break;
                case BaseType.ULong:
                    data = BitConverter.GetBytes((ulong)value);
                    break;
                case BaseType.UInt:
                    data = BitConverter.GetBytes((uint)value);
                    break;
                case BaseType.UShort:
                    data = BitConverter.GetBytes((ushort)value);
                    break;
                case BaseType.Float:
                    data = BitConverter.GetBytes((float)value);
                    break;
                case BaseType.Double:
                    data = BitConverter.GetBytes((double)value);
                    break;
                case BaseType.Decimal:
                    int[] bits = decimal.GetBits((decimal)value);
                    List<byte> bytes = new List<byte>();
                    for (int i = 0; i < bits.Length; i++)
                    {
                        bytes.AddRange(BitConverter.GetBytes(bits[i]));
                    }
                    data = bytes.ToArray();
                    break;
                case BaseType.Bool:
                    data = BitConverter.GetBytes((bool)value);
                    break;
                case BaseType.Char:
                    data = BitConverter.GetBytes((char)value);
                    break;
                case BaseType.Byte:
                    data = new byte[1] { (byte)value };
                    break;
                case BaseType.String | BaseType.HasLength:
                    data = Encoding.GetEncoding(EndcodingCodePage).GetBytes(value.ToString());
                    break;
            }

            if (data != null)
            {
                List<byte> finalData = new List<byte>();

                if (isArray && !baseType.HasFlag(BaseType.Array))
                    baseType |= BaseType.Array;

                if (!isArray || isFirstArrayElement) //if not in array or first element add type 
                {
                    finalData.Add((byte)baseType);
                    if (isArray)
                        finalData.AddRange(BitConverter.GetBytes(arrayLength));
                }

                if (baseType.HasFlag(BaseType.HasLength)) //if has length add byte length
                    finalData.AddRange(BitConverter.GetBytes(data.Length));

                finalData.AddRange(data); //payload
                data = finalData.ToArray();

                return true;
            }
            return false;
        }

        private static bool GetArgumentSize(BaseType type, out int size)
        {
            if(sizes.ContainsKey(type))
            {
                size = sizes[type];
                return true;
            }

            size = -1;
            return false;
        }

        private static bool GetFromBytes(byte[] message, int index, BaseType type, out object value, out int size, int length = 0)
        {
            try
            {
                if (GetArgumentSize(type, out size))
                {
                    if (size == 0)
                    {
                        size = length;
                        if (length == 0)
                            throw new Exception("Invalid length!");
                    }

                    if (!BitConverter.IsLittleEndian)
                        Array.Reverse(message);

                    switch (type)
                    {
                        case BaseType.Long:
                            value = BitConverter.ToInt64(message, index);
                            break;
                        case BaseType.Int:
                            value = BitConverter.ToInt32(message, index);
                            break;
                        case BaseType.Short:
                            value = BitConverter.ToInt16(message, index);
                            break;
                        case BaseType.ULong:
                            value = BitConverter.ToUInt64(message, index);
                            break;
                        case BaseType.UInt:
                            value = BitConverter.ToUInt32(message, index);
                            break;
                        case BaseType.UShort:
                            value = BitConverter.ToUInt16(message, index);
                            break;
                        case BaseType.Float:
                            value = BitConverter.ToSingle(message, index);
                            break;
                        case BaseType.Double:
                            value = BitConverter.ToDouble(message, index);
                            break;
                        case BaseType.Decimal:
                            {
                                value = new decimal(new int[4]
                                {
                                BitConverter.ToInt32(message, index),
                                BitConverter.ToInt32(message, index + 4),
                                BitConverter.ToInt32(message, index + 8),
                                BitConverter.ToInt32(message, index + 12)
                                });
                            }
                            break;
                        case BaseType.Bool:
                            value = BitConverter.ToBoolean(message, index);
                            break;
                        case BaseType.Char:
                            value = BitConverter.ToChar(message, index);
                            break;
                        case BaseType.Byte:
                            value = message[index];
                            break;
                        case BaseType.String:
                            byte[] stringMessage = new byte[size];
                            Array.Copy(message, index, stringMessage, 0, size);
                            value = Encoding.GetEncoding(EndcodingCodePage).GetString(stringMessage);
                            break;
                        default:
                            {
                                value = null;
                                return false;
                            }
                    }
                    return true;
                }
            }
            catch
            {
            }

            size = 0;
            value = null;
            return false;
        }

        private static bool GetFromBytes<T>(byte[] message, int index, BaseType type, out T value, out int size, int length = 0)
        {
            if(GetFromBytes(message, index, type, out object oValue, out size, length))
            {
                if (oValue is T)
                {
                    value = (T)oValue;
                    return true;
                }
            }

            size = 0;
            value = default(T);
            return false;
        }

        public static bool ConvertToCommand(byte[] message, out CommandArgs cmd)
        {
            if(message.Length >= 6)
            {
                int index = 0;
                int size = 0;
                List<object> args = new List<object>();
                List<BaseType> argTypes = new List<BaseType>();

                if (GetFromBytes(message, index, BaseType.Int, out int argsCount, out size)) //Get args count
                {
                    index = size;
                    do
                    {
                        size = 0;
                        if (GetFromBytes(message, index, BaseType.Byte, out byte byteType, out size)) //Get base type
                        {
                            BaseType type = (BaseType)byteType;
                            index += size;

                            bool hasLength = type.HasFlag(BaseType.HasLength);
                            bool isArray = type.HasFlag(BaseType.Array);

                            BaseType baseType = type;
                            baseType &= ~BaseType.HasLength;
                            baseType &= ~BaseType.Array;

                            if (isArray)
                            {
                                args.Add(GetArray(message, ref index, baseType));
                                type &= ~BaseType.HasLength;
                                argTypes.Add(type);
                            }
                            else
                            {
                                args.Add(GetValue(message, ref index, baseType, hasLength));
                                argTypes.Add(baseType);
                            }
                        }
                        else
                        {
                            break;
                        }
                    } while (index < message.Length);

                    if (args.Count > 0 && argTypes.Count > 0)
                    {
                        cmd = new CommandArgs(args[0], argTypes[0], args.GetRange(1, args.Count - 1).ToArray(), argTypes.GetRange(1, argTypes.Count - 1).ToArray());
                        return true;
                    }
                }
            }

            cmd = null;
            return false;
        }

        public static byte[] ConvertToByteArray(object command, params object[] arguments)
        {
            return ConvertArrayToByteArray(command, arguments);
        }

        public static byte[] ConvertArrayToByteArray(object command, object[] arguments)
        {
            if (command == null)
                throw new NullReferenceException();

            List<byte> data = new List<byte>();

            int argCount = 1;
            if (arguments != null)
                argCount += arguments.Length;

            data.AddRange(BitConverter.GetBytes(argCount));

            if (ConvertArgumentToByte(command, out byte[] cmdData))
            {
                data.AddRange(cmdData);
                for (int i = 0; i < arguments.Length; i++)
                {
                    if (ConvertArgumentToByte(arguments[i], out byte[] argData))
                    {
                        data.AddRange(argData);
                    }
                }
            }

            return data.ToArray();
        }

        private static Array GetArray(byte[] message, ref int index, BaseType type)
        {
            int size = 0;
            Array arr = null;
            Type arrType = GetType(type);
            if (arrType == null)
                arrType = typeof(object);

            if (GetFromBytes(message, index, BaseType.Int, out int count, out size))//Get array count
            {
                arr = Array.CreateInstance(arrType, count);
                index += size;
                if (type == BaseType.String) //string array
                {
                    for (int i = 0; i < count; i++)
                    {
                        if (GetFromBytes(message, index, BaseType.Int, out int length, out size)) //Get string length
                        {
                            index += size;
                            if (GetFromBytes(message, index, type, out string value, out size, length))
                            {
                                index += size;
                                arr.SetValue(value, i);
                            }
                        }
                    }
                }
                else //normal array
                {
                    for (int i = 0; i < count; i++)
                    {
                        if (GetFromBytes(message, index, type, out object value, out size))
                        {
                            arr.SetValue(value, i);
                            index += size;
                        }
                    }
                }
            }

            return arr;
        }

        private static object GetValue(byte[] message, ref int index, BaseType type, bool hasLength)
        {
            int length = 0;
            int size = 0;
            if (hasLength)
            {
                if (GetFromBytes(message, index, BaseType.Int, out length, out size))
                {
                    index += size;
                }
            }

            if (GetFromBytes(message, index, type, out object value, out size, length))
            {
                index += size;
                return value;
            }

            return null;
        }
    }
}
