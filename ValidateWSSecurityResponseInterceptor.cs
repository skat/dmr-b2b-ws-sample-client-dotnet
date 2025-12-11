using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace UFSTWSSecuritySample
{
    public class ValidateWSSecurityResponseInterceptor : IClientIinterceptor
    {

        private X509Certificate2 trustedCertificate;
        bool verbose = false;

        public ValidateWSSecurityResponseInterceptor(string trustedCertificatePath)
        {
            init(trustedCertificatePath, false);
        }

        public ValidateWSSecurityResponseInterceptor(string trustedCertificatePath, bool verbose)
        {
            init(trustedCertificatePath, verbose);
        }

        private void init(string trustedCertificatePath, bool verbose)
        {
            var certPem = File.ReadAllText(trustedCertificatePath);
            trustedCertificate = X509Certificate2.CreateFromPem(certPem);
            this.verbose = verbose;
        }


        public void handle(XmlDocument xmlDocument)
        {

            XmlNodeList xmlNodeList = xmlDocument.GetElementsByTagName("wsse:BinarySecurityToken");
            string binarySecurityToken = xmlNodeList[0].InnerText;
            // Wrap in PWM to satisfy X509CertificateLoader.LoadCertificate API:
            string binarySecurityTokenPEM = "-----BEGIN CERTIFICATE-----\n" + binarySecurityToken + "\n-----END CERTIFICATE-----";
            X509Certificate2 x509Certificate2 = X509CertificateLoader.LoadCertificate(Encoding.UTF8.GetBytes(binarySecurityTokenPEM));

            xmlNodeList = xmlDocument.GetElementsByTagName("Signature", "http://www.w3.org/2000/09/xmldsig#");
            XmlElement signature = (XmlElement)xmlNodeList[0];
            SignedXmlWithId signedXml = new SignedXmlWithId(xmlDocument);
            signedXml.LoadXml(signature);

            bool validSignature = signedXml.CheckSignature(x509Certificate2, true);
            if (!validSignature)
            {
                throw new Exception("Invalid signature in response.");
            }

            // Now check that we trust the certificate 
            bool isTrusted = trustedCertificate.Equals(x509Certificate2);
            if (!isTrusted)
            {
                throw new Exception("Certificate is not trusted.");
            }
            if (this.verbose)
            {
            Console.WriteLine("Signature verified: " + validSignature);
            Console.WriteLine("Certificate trusted: " + isTrusted);
            }
        }

        private class SignedXmlWithId : SignedXml
        {
            public SignedXmlWithId(XmlDocument xml) : base(xml)
            {
            }

            public override XmlElement GetIdElement(XmlDocument doc, string id)
            {
                // check to see if it's a standard ID reference
                var idElem = base.GetIdElement(doc, id);

                if (idElem == null)
                {
                    var nsManager = new XmlNamespaceManager(doc.NameTable);
                    nsManager.AddNamespace("wsu",
                        "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");

                    idElem = doc.SelectSingleNode("//*[@wsu:Id=\"" + id + "\"]", nsManager) as XmlElement;
                }

                return idElem;
            }
        }
    }
}