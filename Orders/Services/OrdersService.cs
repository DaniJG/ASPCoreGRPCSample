using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Shippings;

namespace Orders
{
    public class OrderPlacementService : OrderPlacement.OrderPlacementBase
    {
        private readonly ILogger<OrderPlacementService> _logger;
        private readonly ProductShipment.ProductShipmentClient _shippings;

        public OrderPlacementService(ILogger<OrderPlacementService> logger, ProductShipment.ProductShipmentClient shippings)
        {
            _logger = logger;
            _shippings = shippings;
        }

        public override async Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            await this._shippings.SendOrderAsync(new SendOrderRequest
            {
                ProductId = "ABC1234",
                Quantity = 1,
                Address = "Mock Address"
            });

            return new HelloReply
            {
                Message = "Hello " + request.Name
            };
        }
    }
}
