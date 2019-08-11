using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Shippings
{
    public class ShippingsService : ProductShipment.ProductShipmentBase
    {
        private readonly ILogger<ShippingsService> _logger;
        public ShippingsService(ILogger<ShippingsService> logger)
        {
            _logger = logger;
        }

        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HelloReply
            {
                Message = "Hello " + request.Name
            });
        }
    }
}
