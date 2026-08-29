using ECommerce.Application.Customers.Commands.CreateCustomer;
using ECommerce.Application.Customers.Commands.UpgradeCustomerToVip;
using ECommerce.Application.Customers.Queries.GetCustomerById;
using ECommerce.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly IMediator _mediator;

    public CustomersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var customer = await _mediator.Send(
            new GetCustomerByIdQuery(id));

        if (customer is null)
        {
            return NotFound(
                $"Customer with ID {id} not found.");
        }

        return Ok(customer);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateCustomerDto dto)
    {
        try
        {
            var command = new CreateCustomerCommand(
                dto.FullName,
                dto.Email,
                dto.IsVip);

            var customer = await _mediator.Send(command);

            return CreatedAtAction(
                nameof(GetById),
                new { id = customer.Id },
                customer);
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

    [HttpPost("{id}/upgrade-vip")]
    public async Task<IActionResult> UpgradeToVip(int id)
    {
        try
        {
            await _mediator.Send(
                new UpgradeCustomerToVipCommand(id));

            return Ok(new
            {
                message =
                    "Customer upgraded to VIP successfully."
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
}