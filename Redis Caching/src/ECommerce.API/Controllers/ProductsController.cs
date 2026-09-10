using ECommerce.Application.DTOs;
using ECommerce.Application.Products.Commands.CreateProduct;
using ECommerce.Application.Products.Commands.DeleteProduct;
using ECommerce.Application.Products.Commands.ProductViews;
using ECommerce.Application.Products.Commands.UpdateProduct;
using ECommerce.Application.Products.Queries.GetProductById;
using ECommerce.Application.Products.Queries.GetProducts;
using ECommerce.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProductsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _mediator.Send(
                new GetProductsQuery());

            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _mediator.Send(
                new GetProductByIdQuery(id));

            if (product is null)
            {
                return NotFound(
                    $"Product with ID {id} not found.");
            }

            return Ok(product);
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateProductDto dto)
        {
            try
            {
                var product = await _mediator.Send(
                    new CreateProductCommand(
                        dto.Name,
                        dto.SKU,
                        dto.Price,
                        dto.StockQuantity));

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = product.Id },
                    product);
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

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] Product dto)
        {
            try
            {
                var updated = await _mediator.Send(
                    new UpdateProductCommand(
                        id,
                        dto.Name,
                        dto.SKU,
                        dto.Price,
                        dto.StockQuantity));

                if (!updated)
                {
                    return NotFound(
                        $"Product with ID {id} not found.");
                }

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _mediator.Send(
                new DeleteProductCommand(id));

            if (!deleted)
            {
                return NotFound(
                    $"Product with ID {id} not found.");
            }

            return NoContent();
        }

        [HttpPost("{id:int}/view")]
        public async Task<IActionResult> RecordView(int id, CancellationToken cancellationToken)
        {
            await _mediator.Send(
                new RecordProductViewCommand(id),
                cancellationToken);

            return Ok();
        }
    }
}