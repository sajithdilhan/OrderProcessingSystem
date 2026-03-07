using Microsoft.AspNetCore.Mvc;
using PaymentService.Application.Dto;
using PaymentService.Application.Interfaces;

namespace PaymentService.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController(IPaymentService paymentService, ILogger<PaymentsController> logger) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PaymentResponse), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetPayments() //TODO: Implement pagination
    {
        logger.LogInformation("Getting payments.");
        var result = await paymentService.GetAllPaymentsAsync();

        if (!result.IsSuccess)
        {
            return Problem(detail: result.Error!.Message, statusCode: result.Error.Code);
        }

        return Ok(result.Value);
    }
}
