using System.IO;
using System.Xml.Serialization;

namespace NetworkFramework.MessageConverter
{
    public abstract class XMLMessageObject : MessageObject
    {
        public XMLMessageObject(object command, params object[] arguments) : base(command, arguments)
        {
        }

        /// <summary>
        /// Serializes the xml message object to a byte arry which represants the xml string in UTF-8
        /// </summary>
        /// <returns>Returns the xml data in form of a byte array</returns>
        public virtual byte[] ConvertToXmlData()
        {
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(GetType());

                using (StringWriter textWriter = new StringWriter())
                {
                    xmlSerializer.Serialize(textWriter, this);
                    return System.Text.Encoding.UTF8.GetBytes(textWriter.ToString());
                }
            }
            catch
            {
                return null;
            }
        }
        
        /// <summary>
        /// Converts a byte array to a xml message object
        /// </summary>
        /// <typeparam name="T">Type of the xml message object</typeparam>
        /// <param name="data">Data to deserialize</param>
        /// <returns>Returns a xml message object of the given type or null</returns>
        public static T ConvertToObject<T>(byte[] data) where T : XMLMessageObject
        {
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                string xmlString = System.Text.Encoding.UTF8.GetString(data);
                using (TextReader reader = new StringReader(xmlString))
                {
                    object o = xmlSerializer.Deserialize(reader);
                    if (o != null)
                        return (T)o;

                    return null;
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
