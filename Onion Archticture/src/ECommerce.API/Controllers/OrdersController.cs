using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces.IServices;
using ECommerce.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(
        IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(int id)
    {
        var order =
            await _orderService.GetByIdAsync(id);

        if (order is null)
        {
            return NotFound("Order not found.");
        }

        return Ok(order);
    }

    [HttpGet("customer/{customerId}")]
    public async Task<IActionResult> GetCustomerOrders(
        int customerId)
    {
        var orders =
            await _orderService
                .GetCustomerOrdersAsync(customerId);

        return Ok(orders);
    }

    [HttpPost("cancel/{id}")]
    public async Task<IActionResult> CancelOrder(int id)
    {
        try
        {
            await _orderService.CancelAsync(id);

            return Ok(new
            {
                message =
                    "Order cancelled successfully."
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
            var result =
                await _orderService
                    .CheckoutAsync(request);

            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}