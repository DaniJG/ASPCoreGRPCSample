using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using Grpc.Core;
using Grpc.Net.Client;
using Orders;
using Products;

namespace Client
{
    class Program
    {
        private static OrderPlacement.OrderPlacementClient GetOrdersClient()
        {
            // Enable when connecting to server without HTTPS. See: https://github.com/aspnet/AspNetCore.Docs/issues/13120#issuecomment-516598055
            // AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

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

        private static ProductsInventory.ProductsInventoryClient GetProductsClient()
        {
            // Cant use the Grpc.Net.Client.GrpcClient when the service expects client certificates to be provided!
            // This is because as of version 0.1.22-pre2, it doesnt allow specifying the SslCredentials
            // Instead you need to use the Grpc.Core channel and provide the credentials
            //  - It seems to have been rewritten after 0.1.22-pre2, allowing SslCredentials as one of its options: https://github.com/grpc/grpc-dotnet/tree/master/src/Grpc.Net.Client            
            //var basePath = Path.GetDirectoryName(typeof(Program).Assembly.Location);
            //var credentials = new SslCredentials(
            //    File.ReadAllText(Path.Combine(basePath, "ProductCerts", "ca.crt")),
            //    new KeyCertificatePair(
            //        File.ReadAllText(Path.Combine(basePath, "ProductCerts", "client.crt")),
            //        File.ReadAllText(Path.Combine(basePath, "ProductCerts", "client.key"))
            //    ),
            //    (VerifyPeerContext context) => true
            //);
            //var serverName = System.Environment.GetEnvironmentVariable("PRODUCTS_NAME") ?? "localhost:5004";
            //var channel = serverName.Contains("localhost") ?
            //    new Channel(serverName, credentials) :
            //    new Channel(serverName, credentials, new[] {
            //        new ChannelOption(ChannelOptions.SslTargetNameOverride, "localhost") // gRPC server expects host to match cert host . This is only for testing!
            //    });
            //return new ProductsInventory.ProductsInventoryClient(new DefaultCallInvoker(channel));

            // We can use the new HttpClient-based client from Grpc.Net.Client.GrpcClient 
            // as long as the server does not expect client certificates to be provided
            var serverUrl = System.Environment.GetEnvironmentVariable("PRODUCTS_URL") ?? "https://localhost:5004";
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true; // solves both untrusted server CA and different server hostname when running client inside docker
            var httpClient = new HttpClient(handler);
            httpClient.BaseAddress = new Uri(serverUrl);
            return GrpcClient.Create<ProductsInventory.ProductsInventoryClient>(httpClient);
        }

        static async Task Main(string[] args)
        {
            var ordersClient = GetOrdersClient();
            var productsClient = GetProductsClient();

            Console.WriteLine("Welcome to the gRPC client");
            while (true)
            {                
                Console.WriteLine("\nPress key for option:");
                Console.WriteLine("'1' - .NET Orders + Shippings services with server streaming");
                Console.WriteLine("'2' - Node.js Products service");
                Console.WriteLine("(q or Ctrl+C to exit)");
                var key = Console.ReadKey();
                Console.WriteLine("");

                try
                {
                    if (key.KeyChar == '1')
                    {
                        var reply = await ordersClient.CreateOrderAsync(new CreateOrderRequest
                        {
                            ProductId = "ABC1234",
                            Quantity = 1,
                            Address = "Mock Address"
                        });
                        Console.WriteLine($"Created order: {reply.OrderId}");
                        using (var statusReplies = ordersClient.GetOrderStatus(new GetOrderStatusRequest { OrderId = reply.OrderId }))
                        {
                            while (await statusReplies.ResponseStream.MoveNext())
                            {
                                var statusReply = statusReplies.ResponseStream.Current.Status;
                                Console.WriteLine($"Order status: {statusReply}");
                            }
                        }
                    }
                    if (key.KeyChar == '2')
                    {
                        var reply = await productsClient.DetailsAsync(new ProductDetailsRequest { ProductId = "MockProduct" });
                        Console.WriteLine("Got details for: " + reply.Name);
                    }
                    if (key.KeyChar == 'q')
                    {
                        Environment.Exit(0);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }    
    }
}
