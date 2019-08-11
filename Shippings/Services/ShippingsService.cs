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

        public override Task<SendOrderReply> SendOrder(SendOrderRequest request, ServerCallContext context)
        {
            this._logger.LogInformation($"Received order with productId={request.ProductId}, quantity={request.Quantity}, address={request.Address}");
            return Task.FromResult(new SendOrderReply());
        }
    }
}
