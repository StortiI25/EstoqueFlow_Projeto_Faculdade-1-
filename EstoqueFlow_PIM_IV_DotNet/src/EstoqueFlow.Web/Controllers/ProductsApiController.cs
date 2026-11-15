using EstoqueFlow.Web.Data;
using EstoqueFlow.Web.Models;
using EstoqueFlow.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EstoqueFlow.Web.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsApiController(EstoqueFlowContext db, StockService stock) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> Get()
        => Ok(await db.Products.OrderBy(p => p.Name).ToListAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Product>> GetById(int id)
    {
        var product = await db.Products.FindAsync(id);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<Product>> Create(Product input)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        input.Id = 0;
        db.Products.Add(input);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = input.Id }, input);
    }

    public record MovementRequest(int ProductId, int Quantity, string Type, string? Notes);

    [HttpPost("movement")]
    public async Task<IActionResult> Movement(MovementRequest request)
    {
        var result = await stock.RegisterAsync(
            request.ProductId, request.Quantity, request.Type, request.Notes ?? "");
        return result.Success ? Ok(new { message = result.Message })
                              : BadRequest(new { message = result.Message });
    }
}
