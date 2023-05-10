using System;
using System.Xml;

namespace UFSTWSSecuritySample
{
    public class VehicleDetailsPayloadWriter : IPayloadWriter
    {
        public VehicleDetailsPayloadWriter(string vin)
        {
            Vin = vin;
        }
        public void Write(XmlTextWriter writer)
        {
            var now = DateTime.UtcNow.ToString("o").Substring(0, 23) + "Z";
            var transactionId = Guid.NewGuid().ToString();

            writer.WriteStartElement("ns", "USKoeretoejDetaljerVis_I", "http://skat.dk/dmr/2007/05/31/");

            writer.WriteStartElement("ns1", "HovedOplysninger", "http://rep.oio.dk/skat.dk/basis/kontekst/xml/schemas/2006/09/01/");
            writer.WriteStartElement("ns1", "TransaktionIdentifikator", null);
            writer.WriteString(transactionId);
            writer.WriteEndElement(); // TransaktionIdentifikator
            writer.WriteStartElement("ns1", "TransaktionTid", null);
            writer.WriteString(now);
            writer.WriteEndElement(); // TransaktionTid
            writer.WriteEndElement(); // HovedOplysninger

            writer.WriteStartElement("ns", "DatoTidSoegTidspunkt", null);
            writer.WriteString(now);
            writer.WriteEndElement(); // DatoTidSoegTidspunkt

            writer.WriteStartElement("ns", "KoeretoejGenerelIdentifikatorStruktur", null);
            writer.WriteStartElement("ns", "KoeretoejGenerelIdentifikatorValg", null);

            writer.WriteStartElement("ns", "KoeretoejOplysningStelNummer", null);
            writer.WriteString(Vin);
            writer.WriteEndElement(); // KoeretoejOplysningStelNummer

            writer.WriteEndElement(); // KoeretoejGenerelIdentifikatorValg
            writer.WriteEndElement(); // KoeretoejGenerelIdentifikatorStruktur
            writer.WriteEndElement(); // USKoeretoejDetaljerVis_I
        }

        private string Vin { get; }
    }
}
