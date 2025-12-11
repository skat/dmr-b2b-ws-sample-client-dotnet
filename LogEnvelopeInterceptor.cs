using System.Xml;
using System.Xml.Linq;

namespace UFSTWSSecuritySample
{
    public class LogEnvelopeInterceptor : IClientIinterceptor
    {
        
     
        public void handle(XmlDocument document)
        {
            Console.WriteLine("Envelope (with indentation)");
            Console.WriteLine("---------------------------");
            ConsoleWriteEnvelope(document);
        }

        private void ConsoleWriteEnvelope(XmlDocument document)
        {
            var xmlWriterSettings = new XmlWriterSettings();
            xmlWriterSettings.OmitXmlDeclaration = true;
            xmlWriterSettings.Indent = true;
            xmlWriterSettings.NewLineOnAttributes = true;
            using (var stringWriter = new StringWriter())
            using (var xmlTextWriter = XmlWriter.Create(stringWriter, xmlWriterSettings))
            {
                document.WriteTo(xmlTextWriter);
                xmlTextWriter.Flush();
                Console.WriteLine(stringWriter.GetStringBuilder().ToString());
            }
        }
    }
}