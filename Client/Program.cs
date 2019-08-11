using System;
using Grpc.Net.Client;
using System.Net.Http;
using System.Threading.Tasks;
using Orders;


namespace Client
{
    class Program
    {
        private static OrderPlacement.OrderPlacementClient GetClient()
        {
            var serverUrl = System.Environment.GetEnvironmentVariable("GREETER_URL") ?? "https://localhost:5001";

            var handler = new HttpClientHandler();
            if (!serverUrl.Contains("localhost"))
            {
                // NOTE: This is for development purposes only!
                // When running in dev with docker-compose, the address of the server wont be localhost, and so the dev HTTPS cert is invalid
                // This will disable the validation of the HTTPS certificates
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            }
            var httpClient = new HttpClient(handler);
            httpClient.BaseAddress = new Uri(serverUrl);

            return GrpcClient.Create<OrderPlacement.OrderPlacementClient>(httpClient);
        }

        static async Task Main(string[] args)
        {
            var client = GetClient();
            Console.WriteLine("Welcome to the Orders Client!");
            while (true)
            {                
                Console.WriteLine("Enter option:");
                Console.WriteLine("\t- '1' - Place order");
                Console.WriteLine("\t- '2' - Get orders history");
                Console.WriteLine("(Ctrl+C to exit)");
                var key = Console.ReadKey();
                Console.WriteLine("");

                if (key.KeyChar == '1')
                {
                    var reply = await client.SayHelloAsync(new HelloRequest { Name = "GreeterClient" });
                    Console.WriteLine("Greeting: " + reply.Message);
                }
            }
        }    
    }
}
