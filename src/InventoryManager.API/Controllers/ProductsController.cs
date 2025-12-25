using InventoryManager.API.Extensions;
using InventoryManager.Application.DTOs.Common;
using InventoryManager.Application.DTOs.Requests;
using InventoryManager.Application.DTOs.Responses;
using InventoryManager.Application.UseCases.Products.Create;
using InventoryManager.Application.UseCases.Products.Delete;
using InventoryManager.Application.UseCases.Products.GetAll;
using InventoryManager.Application.UseCases.Products.GetById;
using InventoryManager.Application.UseCases.Products.Update;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManager.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class ProductsController : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductResponse>> Create(ICreateProductUseCase useCase, CreateProductRequest request)
    {
        ProductResponse response = await useCase.ExecuteAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProductResponse>>> GetAll(IGetAllProductsUseCase useCase, [FromQuery] GetAllProductsRequest request)
    {
        PagedResult<ProductResponse> result = await useCase.ExecuteAsync(request);
        Response.AddPaginationHeader(result.Meta);

        return Ok(result.Items);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> GetById(IGetProductByIdUseCase useCase, Guid id)
    {
        ProductResponse response = await useCase.ExecuteAsync(id);
        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Update(IUpdateProductUseCase useCase, Guid id, UpdateProductRequest request)
    {
        await useCase.ExecuteAsync(id, request);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(IDeleteProductUseCase useCase, Guid id)
    {
        await useCase.ExecuteAsync(id);
        return NoContent();
    }
}