/* 
 * Author: Marcel Croonenbroeck
 * Date: 11.10.2017
 */
using System;

namespace NetworkFramework.MessageConverter
{
    public struct CommandRule
    {
        public object Command;
        public int MinArgumentCount;
        public int MaxArgumentCount;
        public BaseType[] ArgumentTypes;
        public RuleType Type;

        public CommandRule(object command, int minArgumentCount, int maxArgumentCount, RuleType type, params BaseType[] argumentTypes)
        {
            Command = command;
            MinArgumentCount = minArgumentCount;
            MaxArgumentCount = maxArgumentCount;
            ArgumentTypes = argumentTypes;
            Type = type;
        }

        public CommandRule(object command, int minArgumentCount, params BaseType[] argumentTypes)
        {
            Command = command;
            MinArgumentCount = minArgumentCount;
            MaxArgumentCount = argumentTypes == null ? 0 : argumentTypes.Length;
            ArgumentTypes = argumentTypes;
            Type = RuleType.None;
        }

        public CommandRule(object command, params BaseType[] argumentTypes)
        {
            Command = command;
            MinArgumentCount = argumentTypes == null ? 0 : argumentTypes.Length;
            MaxArgumentCount = argumentTypes == null ? 0 : argumentTypes.Length;
            ArgumentTypes = argumentTypes;
            Type = RuleType.None;
        }

        public CommandRule(object command, RuleType type, params BaseType[] argumentTypes)
        {
            Command = command;
            MinArgumentCount = argumentTypes == null ? 0 : argumentTypes.Length;
            MaxArgumentCount = argumentTypes == null ? 0 : argumentTypes.Length;
            ArgumentTypes = argumentTypes;
            Type = type;
        }

        public CommandRule(object command)
        {
            Command = command;
            MinArgumentCount = 0;
            MaxArgumentCount = 0;
            ArgumentTypes = null;
            Type = RuleType.None;
        }

        /// <summary>
        /// Checks if the given command arguments are valid
        /// </summary>
        /// <param name="args">Command arguments object</param>
        /// <returns>Returns true if the rule is valid</returns>
        public bool IsValidRule(CommandArgs args)
        {
            if(args.Command.Equals(Command))
            {
                if (args.Arguments.Length >= MinArgumentCount && args.Arguments.Length <= MaxArgumentCount)
                {
                    if (ArgumentTypes == null && (args.Arguments == null || args.Arguments.Length == 0))
                        return true;

                    for (int i = 0; i < args.Arguments.Length; i++)
                    {
                        if(args.Types[i] != ArgumentTypes[i])
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
            return false;
        }
    }
}
