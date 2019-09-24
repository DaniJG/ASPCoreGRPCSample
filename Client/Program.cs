using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using Grpc.Core;
using Grpc.Net.Client;
using Orders;
using Products;
using System.Security.Cryptography.X509Certificates;

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

            var channel = GrpcChannel.ForAddress(serverUrl, new GrpcChannelOptions { HttpClient = httpClient });
            return new OrderPlacement.OrderPlacementClient(channel);            
        }

        private static ProductsInventory.ProductsInventoryClient GetProductsClient()
        {
            // Left for reference the implementation needed when using the old Grpc.Core classes directly
            // to instantiate the channel and provide call credentials
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
            //return new ProductsInventory.ProductsInventoryClient(channel);

            // We can build the channel using the new HttpClient-based client from Grpc.Net.Client.GrpcClient 
            // as long as the server does not expect client certificates to be provided
            var serverUrl = System.Environment.GetEnvironmentVariable("PRODUCTS_URL") ?? "https://localhost:5004";
            var basePath = Path.GetDirectoryName(typeof(Program).Assembly.Location);
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true; // solves both untrusted server CA and different server hostname when running client inside docker
            handler.ClientCertificates.Add(new X509Certificate2(Path.Combine(basePath, "ProductCerts", "client.pfx"), "1234"));
            var httpClient = new HttpClient(handler);

            var channel = GrpcChannel.ForAddress(serverUrl, new GrpcChannelOptions { HttpClient = httpClient });
            return new ProductsInventory.ProductsInventoryClient(channel);
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
