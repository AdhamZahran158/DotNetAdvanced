using ECommerce.Application.Carts.Commands.AddToCart;
using ECommerce.Application.Carts.Commands.RemoveFromCart;
using ECommerce.Application.Carts.Queries.GetCart;
using ECommerce.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CartsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CartsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{customerId:int}")]
    public async Task<IActionResult> GetCart(
        int customerId)
    {
        var cart = await _mediator.Send(
            new GetCartQuery(customerId));

        if (cart is null)
        {
            return NotFound(
                $"Cart for customer {customerId} not found.");
        }

        return Ok(cart);
    }

    [HttpPost("{customerId:int}/items")]
    public async Task<IActionResult> AddToCart(
        int customerId,
        [FromBody] AddToCartDto dto)
    {
        try
        {
            await _mediator.Send(
                new AddToCartCommand(
                    customerId,
                    dto));

            return Ok(new
            {
                message = "Product added to cart successfully."
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("{customerId:int}/items/{productId:int}")]
    public async Task<IActionResult> RemoveFromCart(
        int customerId,
        int productId)
    {
        try
        {
            await _mediator.Send(
                new RemoveFromCartCommand(
                    customerId,
                    productId));

            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}