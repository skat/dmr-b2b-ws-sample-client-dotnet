using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Collections;

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

            var command = args[0];
            switch (command)
            {
                case "PayloadWriter":
                    if (args.Length == 2)
                    {
                        Console.WriteLine("Running in 'PayloadWriter' mode.");
                        var service = args[1];
                        IApiClient client = new ApiClient(settings);
                        switch (service)
                        {
                            case "USKoeretoejDetaljerVis":
                                await client.CallService(new VehicleDetailsPayloadWriter("AB12345"), endpoints.USMiljoeordningForBiler);
                                Console.WriteLine("Finished");
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
                        if (File.Exists(responseFilePath))
                        {
                            Console.WriteLine("The file " + responseFilePath + " already exists. Will delete file before doing call.");
                            File.Delete(responseFilePath
                            );
                            Console.WriteLine(responseFilePath + " deleted.");
                        }
                        IApiClient client = new ApiClient(settings, responseFilePath);

                        IPayloadWriter pw = new RequestFromFilePayloadWriter(requestFilePath);
                        await client.CallService(pw, endpoints.USMiljoeordningForBiler);
                        Console.WriteLine("Finished");
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
        }
    }
}
