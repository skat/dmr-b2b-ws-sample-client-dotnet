using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml;

namespace UFSTWSSecuritySample
{
    public class USKoeretoejTekniskDataHentOProcessor
    {

        private XmlDocument doc;

        private XmlNamespaceManager nsmgr;

        public string OutputPath { get; set; }

        public USKoeretoejTekniskDataHentOProcessor(XmlDocument document)
        {
            doc = document;
            nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("ns", "http://skat.dk/dmr/2007/05/31/");
            nsmgr.AddNamespace("ho", "http://rep.oio.dk/skat.dk/basis/kontekst/xml/schemas/2006/09/01/");

             // Build XPaths
            Extract.Add("KoeretoejIdent", "//ns:USKoeretoejTekniskDataHent_O/ns:KoeretoejTekniskDataListe/ns:KoeretoejTekniskData/ns:KoeretoejTekniskDataStruktur/ns:KoeretoejIdent");
            Extract.Add("KoeretoejOplysningStelNummer", "/ns:USKoeretoejTekniskDataHent_O/ns:KoeretoejTekniskDataListe/ns:KoeretoejTekniskData/ns:KoeretoejTekniskDataStruktur/ns:KoeretoejOplysningStruktur/ns:KoeretoejOplysningStelNummer");

            // TypeAttestStruktur:
            Extract.Add("TypeAttestStruktur-KoeretoejArtNummer", "/ns:USKoeretoejTekniskDataHent_O/ns:KoeretoejTekniskDataListe/ns:KoeretoejTekniskData/ns:KoeretoejTekniskDataStruktur/ns:TypeAttestStruktur/ns:KoeretoejArtStruktur/ns:KoeretoejArtNummer");
            Extract.Add("TypeAttestStruktur-KoeretoejArtNavn", "/ns:USKoeretoejTekniskDataHent_O/ns:KoeretoejTekniskDataListe/ns:KoeretoejTekniskData/ns:KoeretoejTekniskDataStruktur/ns:TypeAttestStruktur/ns:KoeretoejArtStruktur/ns:KoeretoejArtNavn");

            // KoeretoejRegistreringGrundlag:
            Extract.Add("KoeretoejRegistreringGrundlag-KoeretoejArtNummer", "/ns:USKoeretoejTekniskDataHent_O/ns:KoeretoejTekniskDataListe/ns:KoeretoejTekniskData/ns:KoeretoejTekniskDataStruktur/ns:KoeretoejRegistreringGrundlag/ns:KoeretoejRegistreringGrundlagStruktur/ns:KoeretoejArtStruktur/ns:KoeretoejArtNummer");
            Extract.Add("KoeretoejRegistreringGrundlag-KoeretoejArtNavn", "/ns:USKoeretoejTekniskDataHent_O/ns:KoeretoejTekniskDataListe/ns:KoeretoejTekniskData/ns:KoeretoejTekniskDataStruktur/ns:KoeretoejRegistreringGrundlag/ns:KoeretoejRegistreringGrundlagStruktur/ns:KoeretoejArtStruktur/ns:KoeretoejArtNavn");
            Extract.Add("KoeretoejRegistreringGrundlag-KoeretoejAnvendelseNummer", "/ns:USKoeretoejTekniskDataHent_O/ns:KoeretoejTekniskDataListe/ns:KoeretoejTekniskData/ns:KoeretoejTekniskDataStruktur/ns:KoeretoejRegistreringGrundlag/ns:KoeretoejRegistreringGrundlagStruktur/ns:KoeretoejAnvendelseStruktur/ns:KoeretoejAnvendelseNummer");
            Extract.Add("KoeretoejRegistreringGrundlag-KoeretoejAnvendelseNavn", "/ns:USKoeretoejTekniskDataHent_O/ns:KoeretoejTekniskDataListe/ns:KoeretoejTekniskData/ns:KoeretoejTekniskDataStruktur/ns:KoeretoejRegistreringGrundlag/ns:KoeretoejRegistreringGrundlagStruktur/ns:KoeretoejAnvendelseStruktur/ns:KoeretoejAnvendelseNavn");

            Extract.Add("KoeretoejOplysningTotalVaegt", "/ns:USKoeretoejTekniskDataHent_O/ns:KoeretoejTekniskDataListe/ns:KoeretoejTekniskData/ns:KoeretoejTekniskDataStruktur/ns:KoeretoejOplysningStruktur/ns:KoeretoejOplysningTotalVaegt");
            Extract.Add("KoeretoejOplysningTekniskTotalVaegt", "/ns:USKoeretoejTekniskDataHent_O/ns:KoeretoejTekniskDataListe/ns:KoeretoejTekniskData/ns:KoeretoejTekniskDataStruktur/ns:KoeretoejOplysningStruktur/ns:KoeretoejOplysningTekniskTotalVaegt");

        }

        public bool HasErrorCode(String ErrorCode)
        {
            LinkedList<String> errors = GetErrors();
            LinkedListNode<String> current = errors.FindLast(ErrorCode);
            return current != null;
        }

        public LinkedList<String> GetErrors()
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

        private Dictionary<String, String> Extract = new Dictionary<String, String>();

        public void Process()
        {
            if (!HasErrors())
            {
                // Build CSV record with value
                StringBuilder sb = new StringBuilder();
                int dictSize = Extract.Count;
                int i = 1;
                foreach (KeyValuePair<String, String> entry in Extract)
                {
                    // entry.Key contains the field name
                    XmlNode value = doc.SelectSingleNode(entry.Value, nsmgr);
                    sb.Append(value.InnerText);
                    if (i < dictSize)
                    {
                        sb.Append(";");
                    }
                    i++;

                }

                if (OutputPath != null)
                {
                    using (StreamWriter sw = File.AppendText(OutputPath))
                    {
                        sw.WriteLine(sb.ToString());
                    }
                }
                else
                {
                    Console.WriteLine(sb.ToString());
                }
            }
        }
    }
}
