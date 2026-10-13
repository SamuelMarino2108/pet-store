using Microsoft.AspNetCore.Mvc;
using Store.Application.Catalog;
using Store.Application.Common;

namespace Store.Api.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductsController(ICatalogService catalog) : ControllerBase
{
    /// <summary>Searches the catalog with filters, sorting and paging.</summary>
    [HttpGet]
    [ProducesResponseType<PagedResult<ProductSummaryDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<ProductSummaryDto>>> Search(
        [FromQuery] ProductSearchCriteria criteria,
        CancellationToken cancellationToken) =>
        await catalog.SearchProductsAsync(criteria, cancellationToken);

    /// <summary>Gets a product by its slug.</summary>
    [HttpGet("{slug}")]
    [ProducesResponseType<ProductDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDetailDto>> GetBySlug(string slug, CancellationToken cancellationToken) =>
        await catalog.GetProductBySlugAsync(slug, cancellationToken) is { } product
            ? product
            : NotFound();
}
