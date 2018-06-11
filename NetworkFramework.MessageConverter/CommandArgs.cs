using System;

namespace NetworkFramework.MessageConverter
{ 
    public class CommandArgs : EventArgs
    {
        public object Command;
        public BaseType CommandType;
        public object[] Arguments;
        public BaseType[] Types;

        public CommandArgs(object command, BaseType commandType, object[] arguments, BaseType[] types)
        {
            Command = command;
            CommandType =commandType;
            Arguments = arguments;
            Types = types;
        }
    }
}
