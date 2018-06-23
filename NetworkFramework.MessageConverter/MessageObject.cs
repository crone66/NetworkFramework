namespace NetworkFramework.MessageConverter
{
    public abstract class MessageObject
    {
        protected object command;
        protected object[] arguments;

        public MessageObject(object command, params object[] arguments)
        {
            this.command = command;
            this.arguments = arguments;
        }

        public virtual byte[] ConvertToData()
        {
            return MessageConverter.ConvertArrayToByteArray(command, arguments);
        }
    }
}
