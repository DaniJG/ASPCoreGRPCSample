using System;
using Grpc.Net.Client;
using System.Net.Http;
using System.Threading.Tasks;
using Orders;
using Shippings;

namespace Client
{
    class Program
    {
        private static OrderPlacement.OrderPlacementClient GetOrdersClient()
        {
            var serverUrl = System.Environment.GetEnvironmentVariable("ORDERS_URL") ?? "https://localhost:5001";

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

        private static ProductShipment.ProductShipmentClient GetShippingsClient()
        {
            var serverUrl = System.Environment.GetEnvironmentVariable("SHIPPINGS_URL") ?? "https://localhost:5003";

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

            return GrpcClient.Create<ProductShipment.ProductShipmentClient>(httpClient);
        }

        static async Task Main(string[] args)
        {
            var ordersClient = GetOrdersClient();
            var shippingsClient = GetShippingsClient();
            Console.WriteLine("Welcome to the Orders Client!");
            while (true)
            {                
                Console.WriteLine("Enter option:");
                Console.WriteLine("\t- '1' - order service");
                Console.WriteLine("\t- '2' - shippings service");
                Console.WriteLine("(Ctrl+C to exit)");
                var key = Console.ReadKey();
                Console.WriteLine("");
                
                if (key.KeyChar == '1')
                {
                    var reply = await ordersClient.SayHelloAsync(new Orders.HelloRequest { Name = "GreeterClient" });
                    Console.WriteLine("Greeting: " + reply.Message);
                } else if (key.KeyChar == '2')
                {
                    var reply = await shippingsClient.SayHelloAsync(new Shippings.HelloRequest { Name = "GreeterClient" });
                    Console.WriteLine("Greeting: " + reply.Message);
                }
            }
        }    
    }
}
