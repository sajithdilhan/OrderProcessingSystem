using OrderService.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace OrderService.Application.Dto;

public class OrderResponse
{
    public int OrderId { get; set; }

    public decimal Amount { get; set; }

    public string CustomerEmail { get; set; } = string.Empty;

    public static OrderResponse ToDto(Order order)
    {
        return new OrderResponse
        {
             Amount = order.Amount,
             OrderId = order.OrderId,
             CustomerEmail = order.CustomerEmail
        };
    }
}
