using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Collections;
using System.Xml;

namespace UFSTWSSecuritySample
{
    class Program
    {
        static async Task Main(string[] args)
        {


            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .AddEnvironmentVariables()
                .Build();

            Settings settings = configuration.GetSection("Settings").Get<Settings>();
            Endpoints endpoints = configuration.GetSection("Endpoints").Get<Endpoints>();

            Console.WriteLine($"Path to PCKS#12 file = {settings.PathPKCS12}");
            Console.WriteLine($"Path to PEM file = {settings.PathPEM}");

            if (!File.Exists(settings.PathPKCS12))
            {
                Console.WriteLine("Cannot find " + settings.PathPKCS12);
                Console.WriteLine("Aborting run...");
                return;
            }

            if (!File.Exists(settings.PathPEM))
            {
                Console.WriteLine("Cannot find " + settings.PathPEM);
                Console.WriteLine("Aborting run...");
                return;
            }

            if (args.Length == 0)
            {
                Console.WriteLine("Invalid args");
                return;
            }

            bool verbose = false;
            var requestInterceptors = new LinkedList<IClientIinterceptor>();
            if (settings.LogRequest)
            {
                requestInterceptors.AddLast(new LogEnvelopeInterceptor());
            }
            var responseInterceptors = new LinkedList<IClientIinterceptor>();
            if (settings.LogResponse)
            {
                responseInterceptors.AddLast(new LogEnvelopeInterceptor());
                verbose = true;
            }
        
            responseInterceptors.AddLast(new ValidateWSSecurityResponseInterceptor(settings.PathPEM, verbose));
            var command = args[0];
            switch (command)
            {
                case "ExtractViaKIDLookup":
                    if (args.Length == 3)
                    {
                        Console.WriteLine("Running in 'ExtractKID' mode.");
                        var id = args[1];
                        var next = args[2];
                        VehicleIdType vehicleIdType = VehicleIdType.KID;
                        IApiClient client = new ApiClient(settings);

                        long m = long.Parse(next);
                        long initialKid = long.Parse(id);
                        for (int i = 1; i <= m; i++)
                        {
                            string nextKid = (initialKid - 1 + i).ToString();
                            Console.WriteLine("nextKid=" + nextKid);
                            XmlDocument response = await client.CallService(new VehicleDetailsPayloadWriter(nextKid, vehicleIdType), requestInterceptors, responseInterceptors, endpoints.USMiljoeordningForBiler);
                            if (response != null)
                            {
                                USKoeretoejDetaljerVisResponsePayloadProcessor proc = new USKoeretoejDetaljerVisResponsePayloadProcessor(response);
                                if (!proc.HasErrors())
                                {
                                    proc.Process();
                                }
                                else
                                {
                                    Console.WriteLine("nextKid=" + nextKid + " has errors:");
                                }
                            }
                        }
                    }
                    break;
                case "PayloadWriter":
                    if (args.Length == 4)
                    {
                        Console.WriteLine("Running in 'PayloadWriter' mode.");
                        var service = args[1];
                        var type = args[2];
                        var id = args[3];
                        VehicleIdType vehicleIdType = Enum.Parse<VehicleIdType>(type);
                        IApiClient client = new ApiClient(settings);
                        switch (service)
                        {
                            case "USKoeretoejDetaljerVis":
                                XmlNode node = await client.CallService(new VehicleDetailsPayloadWriter(id, vehicleIdType), requestInterceptors, responseInterceptors, endpoints.USMiljoeordningForBiler);
                                break;
                        }
                    }
                    break;
                case "RequestFromFilePayloadWriter":
                    if (args.Length == 4)
                    {
                        Console.WriteLine("Running in 'RequestFromFilePayloadWriter' mode.");
                        var serviceEndpointKey = args[1];
                        var requestFilePath = args[2];
                        var responseFilePath = args[3];
                        Hashtable hashtable = new Hashtable();
                        // Convert app settings endpoint to lookup table
                        hashtable.Add("USMiljoeordningForBiler", endpoints.USMiljoeordningForBiler);
                        hashtable.Add("USForsikring", endpoints.USForsikring);
                        hashtable.Add("USKoeretoejDetaljerVis", endpoints.USKoeretoejDetaljerVis);

                        String endpoint = (string)hashtable[serviceEndpointKey];
                        if (String.IsNullOrEmpty(endpoint))
                        {
                            Console.WriteLine("Cannot endpoint configuration for: " + serviceEndpointKey);
                            return;
                        }
                        if (!File.Exists(requestFilePath))
                        {
                            Console.WriteLine("Cannot find " + requestFilePath);
                            return;
                        }
                        responseInterceptors.AddLast(new SavePayloadToFileInteceptor(responseFilePath));
                        IApiClient client = new ApiClient(settings);
                        IPayloadWriter pw = new RequestFromFilePayloadWriter(requestFilePath);
                        await client.CallService(pw, requestInterceptors, responseInterceptors, endpoints.USMiljoeordningForBiler);

                        return;
                    }
                    break;
                default:
                    Console.WriteLine("Invalid command");
                    Console.WriteLine("Example:");
                    Console.WriteLine("dotnet run PayloadWriter [USKoeretoejDetaljerVis]");
                    Console.WriteLine("dotnet run Generic [serviceEndpointKey] [request file] [response file]");
                    break;
            }
            Console.WriteLine("Finished");
        }
    }
}
