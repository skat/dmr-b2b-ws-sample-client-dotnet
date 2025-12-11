using System.Xml;

namespace UFSTWSSecuritySample
{
    public interface IClientIinterceptor
    {
        void handle(XmlDocument document);
    }

}