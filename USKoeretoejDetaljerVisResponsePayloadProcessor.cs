using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml;

namespace UFSTWSSecuritySample
{
    public class USKoeretoejDetaljerVisResponsePayloadProcessor
    {

        private XmlDocument doc;

        private XmlNamespaceManager nsmgr;

        public USKoeretoejDetaljerVisResponsePayloadProcessor(XmlDocument document)
        {
            doc = document;
            nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("ns", "http://skat.dk/dmr/2007/05/31/");
            nsmgr.AddNamespace("ho", "http://rep.oio.dk/skat.dk/basis/kontekst/xml/schemas/2006/09/01/");
        }

        public bool HasErrorCode(String ErrorCode)
        {
            LinkedList<String> errors = GetErrors();
            LinkedListNode<String> current = errors.FindLast(ErrorCode);
            return current != null;
        }

        private LinkedList<String> GetErrors()
        {
            LinkedList<String> errors = new LinkedList<String>();
            XmlNodeList SvarStrukturList = doc.SelectNodes("/ns:USKoeretoejDetaljerVis_O/ho:HovedOplysningerSvar/ho:SvarStruktur", nsmgr);
            foreach (XmlNode SvarStruktur in SvarStrukturList)
            {
                XmlNode FejlIdentifikatorNode = SvarStruktur.SelectSingleNode("ho:FejlStruktur/ho:FejlIdentifikator", nsmgr);
                if (FejlIdentifikatorNode != null)
                {
                    errors.AddLast(FejlIdentifikatorNode.InnerText);
                }
            }
            return errors;
        }

        public bool HasErrors()
        {
            LinkedList<String> errors = GetErrors();
            return errors.Count > 0;
        }

        public void Process()
        {
            if (!HasErrors())
            {
                XmlNode kid = doc.SelectSingleNode("/ns:USKoeretoejDetaljerVis_O/ns:KoeretoejDetaljerVisSamling/ns:KoeretoejDetaljerVis/ns:KoeretoejOplysningStruktur/ns:KoeretoejFastKombination/ns:KoeretoejIdent", nsmgr);
                XmlNode vin = doc.SelectSingleNode("/ns:USKoeretoejDetaljerVis_O/ns:KoeretoejDetaljerVisSamling/ns:KoeretoejDetaljerVis/ns:KoeretoejOplysningStruktur/ns:KoeretoejOplysningStelNummer", nsmgr);
                Console.WriteLine(kid.InnerText + ";" + vin.InnerText);
            }
        }

    }
}
