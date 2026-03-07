using MassTransit;
using OrderService.Application.Dto;
using OrderService.Application.Interfaces;
using Shared.Contracts.Common;
using Shared.Contracts.Events;
using System.Net;

namespace OrderService.Application.Services;

public class OrderService(IOrderRepository orderRepository, IPublishEndpoint publishEndpoint, ILogger<OrderService> logger) : IOrderService
{
    public async Task<Result<IEnumerable<OrderResponse>>> GetAllOrdersAsync()
    {
        var orders = await orderRepository.GetAllOrdersAsync();
        if (orders is null)
        {
            logger.LogWarning("No orders found!");
            return Result<IEnumerable<OrderResponse>>.Failure(new Error((int)HttpStatusCode.BadRequest, "No orders found!"));
        }

        logger.LogInformation("Returning orders.");
        var dtos = orders.Select(p => OrderResponse.ToDto(p));
        return Result<IEnumerable<OrderResponse>>.Success(dtos);
    }

    public async Task<Result<int>> CreateOrderAsync(OrderRequest request)
    {
        var order = OrderRequest.ToOrder(request);
        order = await orderRepository.CreateOrderAsync(order);
        if (order.OrderId == 0)
        {
            logger.LogError("Failed to create order for customer:{CustomerEmail}", request.CustomerEmail);
            return Result<int>.Failure(new Error((int)HttpStatusCode.InternalServerError, "Failed to create order."));
        }

        logger.LogInformation("Publishing order created event: {OrderId}", order.OrderId);
        await publishEndpoint.Publish(new OrderCreatedEvent(order.OrderId, order.Amount, order.CustomerEmail, order.OrderDate));
        logger.LogInformation("published event: {OrderId}", order.OrderId);

        logger.LogInformation("Order created successfully for customer:{CustomerEmail}", request.CustomerEmail);
        return Result<int>.Success(order.OrderId);
    }
}