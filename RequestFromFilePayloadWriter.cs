using System;
using System.Xml;

namespace UFSTWSSecuritySample
{
    public class RequestFromFilePayloadWriter : IPayloadWriter
    {
        public RequestFromFilePayloadWriter(string filePath)
        {
            FilePath = filePath;
        }
        public void Write(XmlTextWriter writer)
        {
            XmlDocument xmlDcoument = new XmlDocument();
            xmlDcoument.Load(@FilePath);
            xmlDcoument.WriteTo(writer);
        }

        private string FilePath { get; }
    }
}
