using Microsoft.AspNetCore.Mvc;
using OrderService.Application.Dto;
using OrderService.Application.Interfaces;
using System.Text.Json;

namespace OrderService.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrdersController(IOrderService orderService, ILogger<OrdersController> logger) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(OrderResponse), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetOrders() //TODO: Implement pagination
    {
        logger.LogInformation("Getting orders.");
        var result = await orderService.GetAllOrdersAsync();

        if (!result.IsSuccess)
        {
            return Problem(detail: result.Error!.Message, statusCode: result.Error.Code);
        }

        return Ok(result.Value);
    }

    [HttpPost]
    [ProducesResponseType(typeof(OrderResponse), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> CreateOrders(OrderRequest request) 
    {
        logger.LogInformation("Creating order :{Order}", JsonSerializer.Serialize(request));
        var result = await orderService.CreateOrderAsync(request);

        if (!result.IsSuccess)
        {
            return Problem(detail: result.Error!.Message, statusCode: result.Error.Code);
        }

        return CreatedAtAction(nameof(GetOrders), new { id = result.Value }, result.Value);
    }
}
