using Dive_Deep.Models;
using Dive_Deep.Services;
using Dive_Deep.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Dive_Deep.Controllers;

public class ProductsController : Controller
{
    private readonly IProductService _products;

    public ProductsController(IProductService products) => _products = products;

    [HttpGet]
    public async Task<IActionResult> Index(
        string? search,
        string? category,
        decimal? minDailyRate,
        decimal? maxDailyRate,
        CancellationToken cancellationToken)
    {
        var model = await BuildCatalogAsync(search, category, minDailyRate, maxDailyRate, cancellationToken);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Category(
        string category,
        string? search,
        decimal? minDailyRate,
        decimal? maxDailyRate,
        CancellationToken cancellationToken)
    {
        var categories = await _products.GetCategoryNamesAsync(cancellationToken);
        var matchingCategory = categories.FirstOrDefault(name =>
            string.Equals(name, category, StringComparison.OrdinalIgnoreCase));

        if (matchingCategory is null)
        {
            return NotFound();
        }

        var model = await BuildCatalogAsync(search, matchingCategory, minDailyRate, maxDailyRate, cancellationToken);
        return View("Index", model);
    }

    // Keep the original MVC URLs working while the category links use one shared view.
    public Task<IActionResult> BCD_Products(CancellationToken cancellationToken) =>
        Category(ProductCategoryNames.Bcd, null, null, null, cancellationToken);

    public Task<IActionResult> Dykkedragter_Products(CancellationToken cancellationToken) =>
        Category(ProductCategoryNames.Wetsuits, null, null, null, cancellationToken);

    private async Task<ProductCatalogViewModel> BuildCatalogAsync(
        string? search,
        string? category,
        decimal? minimumDailyRate,
        decimal? maximumDailyRate,
        CancellationToken cancellationToken)
    {
        var filterError = minimumDailyRate > maximumDailyRate
            ? "Mindste dagspris skal være lavere end eller lig med højeste dagspris."
            : null;

        if (filterError is not null)
        {
            minimumDailyRate = null;
            maximumDailyRate = null;
        }

        var products = await _products.SearchProductsAsync(
            search,
            category,
            minimumDailyRate,
            maximumDailyRate,
            cancellationToken);
        var categoryNames = await _products.GetCategoryNamesAsync(cancellationToken);
        var packageVariants = await _products.GetBookingChoicesAsync(cancellationToken);

        ViewData["Title"] = string.IsNullOrWhiteSpace(category) ? "Produkter" : category;
        return new ProductCatalogViewModel
        {
            Products = products,
            PackageVariants = packageVariants,
            Categories = categoryNames,
            Search = search,
            Category = category,
            MinimumDailyRate = minimumDailyRate,
            MaximumDailyRate = maximumDailyRate,
            FilterError = filterError
        };
    }
}
