using Dive_Deep.Api.Contracts;
using Dive_Deep.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dive_Deep.Api.Controllers;

[ApiController]
[Route("api/v1/products")]
[Produces("application/json")]
public sealed class ProductsApiController(IProductService products) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductResponse>>> GetProducts(
        string? search,
        string? category,
        decimal? minDailyRate,
        decimal? maxDailyRate,
        CancellationToken cancellationToken)
    {
        if (minDailyRate > maxDailyRate)
        {
            return BadRequest(new { message = "Mindste dagspris skal være lavere end eller lig med højeste dagspris." });
        }

        var catalog = await products.SearchProductsAsync(
            search,
            category,
            minDailyRate,
            maxDailyRate,
            cancellationToken);
        return Ok(catalog.Select(product => product.ToApi()).ToArray());
    }

    [HttpGet("categories")]
    public async Task<ActionResult<IReadOnlyList<string>>> GetCategories(CancellationToken cancellationToken) =>
        Ok(await products.GetCategoryNamesAsync(cancellationToken));
}
