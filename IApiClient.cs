using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml;

namespace UFSTWSSecuritySample
{
    public interface IApiClient
    {
        Task<XmlDocument> CallService(IPayloadWriter payloadWriter, LinkedList<IClientIinterceptor> requestInteceptors, LinkedList<IClientIinterceptor> responseInteceptors, string endpoint);
    }
}
