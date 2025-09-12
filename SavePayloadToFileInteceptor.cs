using System.Xml;
using System.Xml.Linq;

namespace UFSTWSSecuritySample
{
    public class SavePayloadToFileInteceptor : IClientIinterceptor
    {

        private string filePath;

        public SavePayloadToFileInteceptor(string filename2)
        {
            filePath = filename2;
        }

        public void handle(XmlDocument document)
        {
            if (File.Exists(filePath))
            {
                Console.WriteLine("The file " + filePath + " already exists. File will be replaced.");
                File.Delete(filePath);
            }
            var xmlWriterSettings = new XmlWriterSettings();
            xmlWriterSettings.OmitXmlDeclaration = false;
            xmlWriterSettings.Indent = true;
            xmlWriterSettings.NewLineOnAttributes = true;
            using (var stringWriter = new StringWriter())
            using (var xmlTextWriter = XmlWriter.Create(stringWriter, xmlWriterSettings))
            {
                var node = ExtractSOAPBody(document);
                node.WriteTo(xmlTextWriter);
                xmlTextWriter.Flush();
                using (StreamWriter writer = System.IO.File.AppendText(filePath))
                {
                    writer.WriteLine(stringWriter.GetStringBuilder().ToString());
                }
            }
             Console.WriteLine("Saved payload to file " + filePath + ".");
        }

        public XmlNode ExtractSOAPBody(XmlDocument xmlDocument)
        {
            {
                var nsmgr = new XmlNamespaceManager(xmlDocument.NameTable);
                nsmgr.AddNamespace("soapenv", "http://schemas.xmlsoap.org/soap/envelope/");
                nsmgr.AddNamespace("ns", "http://skat.dk/dmr/2007/05/31/");
                XmlNode node = xmlDocument.DocumentElement.SelectSingleNode("/soapenv:Envelope/soapenv:Body/ns:*", nsmgr);
                return node;
            }
        }

    }
}