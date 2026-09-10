using ECommerce.Application.DTOs;
using ECommerce.Application.Orders.Commands.CancelOrder;
using ECommerce.Application.Orders.Commands.Checkout;
using ECommerce.Application.Orders.Commands.QueueOrderPdf;
using ECommerce.Application.Orders.Queries.GetCustomerOrders;
using ECommerce.Application.Orders.Queries.GetOrderById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(int id)
    {
        var order = await _mediator.Send(
            new GetOrderByIdQuery(id));

        if (order is null)
            return NotFound();

        return Ok(order);
    }

    [HttpGet("customer/{customerId}")]
    public async Task<IActionResult> GetCustomerOrders(
        int customerId)
    {
        var orders = await _mediator.Send(
            new GetCustomerOrdersQuery(customerId));

        return Ok(orders);
    }

    [HttpPost("cancel/{id}")]
    public async Task<IActionResult> CancelOrder(int id)
    {
        try
        {
            await _mediator.Send(
                new CancelOrderCommand(id));

            return Ok(new
            {
                message = "Order cancelled successfully"
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout(
        [FromBody] CreateOrderDto request)
    {
        try
        {
            var command = new CheckoutCommand(
                request.CustomerId,
                request.Items,
                request.CouponCode);

            var result = await _mediator.Send(command);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id:int}/generate-pdf")]
    public async Task<IActionResult> GeneratePdf(
        int id,
        CancellationToken cancellationToken)
    {
        var outputDirectory =
            Path.Combine(
                Directory.GetCurrentDirectory(),
                "GeneratedPdfs");

        Directory.CreateDirectory(outputDirectory);

        var outputPath =
            Path.Combine(
                outputDirectory,
                $"Order-{id}.pdf");

        await _mediator.Send(
            new QueueOrderPdfCommand(
                id,
                outputPath),
            cancellationToken);

        return Accepted(new
        {
            Message = "PDF generation has been queued.",
            OrderId = id
        });
    }
}