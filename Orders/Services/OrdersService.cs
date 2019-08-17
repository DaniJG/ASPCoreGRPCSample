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

        public override async Task<CreateOrderReply> CreateOrder(CreateOrderRequest request, ServerCallContext context)
        {
            var orderId = Guid.NewGuid().ToString();
            await this._shippings.SendOrderAsync(new SendOrderRequest
            {
                ProductId = request.ProductId,
                Quantity = request.Quantity,
                Address = request.Address
            });
            this._logger.LogInformation($"Created order {orderId} with productId={request.ProductId}, quantity={request.Quantity}, address={request.Address}");
            return new CreateOrderReply {
                OrderId = orderId
            };
        }

        public override async Task GetOrderStatus(GetOrderStatusRequest request, IServerStreamWriter<GetOrderStatusResponse> responseStream, ServerCallContext context)
        {
            await responseStream.WriteAsync(new GetOrderStatusResponse { Status = "Created" });

            await Task.Delay(500);

            await responseStream.WriteAsync(new GetOrderStatusResponse { Status = "Validated" });

            await Task.Delay(1000);

            await responseStream.WriteAsync(new GetOrderStatusResponse { Status = "Dispatched" });
        }
    }
}
