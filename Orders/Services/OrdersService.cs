using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Orders
{
    public class OrderPlacementService : OrderPlacement.OrderPlacementBase
    {
        private readonly ILogger<OrderPlacementService> _logger;
        public OrderPlacementService(ILogger<OrderPlacementService> logger)
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
