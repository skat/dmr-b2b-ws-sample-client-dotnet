using System;
using System.Xml;

namespace UFSTWSSecuritySample
{

    public enum VehicleIdType
    {
        VIN,
        KID,
        REG
    }

    public class VehicleDetailsPayloadWriter : IPayloadWriter
    {
        public VehicleDetailsPayloadWriter(string id, VehicleIdType type)
        {
            Id = id;
            Type = type;
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
            if (Type == VehicleIdType.VIN) {
                writer.WriteStartElement("ns", "KoeretoejOplysningStelNummer", null);
                writer.WriteString(Id);
                writer.WriteEndElement(); // KoeretoejOplysningStelNummer
            }
            if (Type == VehicleIdType.KID) {
                writer.WriteStartElement("ns", "KoeretoejIdent", null);
                writer.WriteString(Id);
                writer.WriteEndElement(); // KoeretoejIdent

            }
            if (Type == VehicleIdType.REG) {
                writer.WriteStartElement("ns", "RegistreringNummerNummer", null); // TODO
                writer.WriteString(Id);
                writer.WriteEndElement(); // RegistreringNummerNummer

            }
            writer.WriteEndElement(); // KoeretoejGenerelIdentifikatorValg
            writer.WriteEndElement(); // KoeretoejGenerelIdentifikatorStruktur
            writer.WriteEndElement(); // USKoeretoejDetaljerVis_I
        }

        private string Id { get; }

        private VehicleIdType Type { get; }

    }
}
