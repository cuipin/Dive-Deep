using Dive_Deep.Models;
using Dive_Deep.Services;
using Dive_Deep.Services.Contracts;
using Dive_Deep.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Dive_Deep.Controllers;

[Authorize(Roles = "Admin")]
public class AdminProductsController : Controller
{
    private const long MaximumImageBytes = 5 * 1024 * 1024;
    private readonly IProductService _products;
    private readonly IWebHostEnvironment _environment;

    public AdminProductsController(IProductService products, IWebHostEnvironment environment)
    {
        _products = products;
        _environment = environment;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var products = await _products.GetManageableProductsAsync(cancellationToken);
        return View(products);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = new AdminProductCreateViewModel();
        await PopulateCategoriesAsync(model, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        AdminProductCreateViewModel input,
        CancellationToken cancellationToken)
    {
        await PopulateCategoriesAsync(input, cancellationToken);

        var hasNewCategory = !string.IsNullOrWhiteSpace(input.NewCategoryName);
        if (!input.CategoryId.HasValue && !hasNewCategory)
        {
            ModelState.AddModelError(nameof(input.CategoryId), "Vælg en kategori eller skriv en ny kategori.");
        }
        else if (input.CategoryId.HasValue && hasNewCategory)
        {
            ModelState.AddModelError(nameof(input.NewCategoryName), "Vælg enten en eksisterende kategori eller opret en ny.");
        }

        if (input.Variants
            .Where(variant => !string.IsNullOrWhiteSpace(variant.OptionLabel))
            .GroupBy(variant => variant.OptionLabel.Trim(), StringComparer.OrdinalIgnoreCase)
            .Any(group => group.Count() > 1))
        {
            ModelState.AddModelError(nameof(input.Variants), "Variantnavne skal være forskellige inden for samme produkt.");
        }

        var (imageExtension, imageError) = await GetImageExtensionAsync(input.Image, cancellationToken);
        if (imageError is not null)
        {
            ModelState.AddModelError(nameof(input.Image), imageError);
        }

        if (!ModelState.IsValid || imageExtension is null || input.Image is null)
        {
            return View(input);
        }

        var imageName = $"{Guid.NewGuid():N}{imageExtension}";
        var imageDirectory = Path.Combine(
            _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot"),
            "lib",
            "Images");
        Directory.CreateDirectory(imageDirectory);
        var imagePath = Path.Combine(imageDirectory, imageName);

        try
        {
            await using (var destination = new FileStream(
                imagePath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920,
                useAsync: true))
            {
                await input.Image.CopyToAsync(destination, cancellationToken);
            }

            var request = new ProductCreateRequest
            {
                CategoryId = input.CategoryId,
                NewCategoryName = input.NewCategoryName,
                Brand = input.Brand,
                Model = input.Model,
                ImageFileName = imageName,
                Variants = input.Variants.Select(variant => new ProductVariantCreateRequest
                {
                    OptionLabel = variant.OptionLabel,
                    Size = variant.Size,
                    Gender = variant.Gender,
                    EquipmentType = variant.EquipmentType,
                    ThicknessMm = variant.ThicknessMm,
                    VolumeLiters = variant.VolumeLiters,
                    FirstStage = variant.FirstStage,
                    SecondStage = variant.SecondStage,
                    Octopus = variant.Octopus,
                    DailyRate = variant.DailyRate,
                    InitialUnitCount = variant.InitialUnitCount
                }).ToArray()
            };

            var result = await _products.CreateProductAsync(request, cancellationToken);
            if (!result.Succeeded)
            {
                TryDeleteImage(imagePath);
                AddCreationError(result.Status);
                return View(input);
            }

            TempData["StatusMessage"] = $"Produktet {result.Product!.Brand} {result.Product.Model} er oprettet.";
            return RedirectToAction(nameof(Index));
        }
        catch
        {
            TryDeleteImage(imagePath);
            throw;
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _products.DeleteProductAsAdminAsync(id, cancellationToken);
        switch (result.Status)
        {
            case ProductDeleteStatus.Deleted:
                TryDeleteProductImage(result.UnusedImageFileName);
                TempData["StatusMessage"] = "Produktet og dets lagerenheder blev slettet.";
                break;
            case ProductDeleteStatus.Archived:
                TempData["StatusMessage"] = "Produktet blev arkiveret, fordi en booking eller kurv henviser til det. Det vises ikke længere i kataloget, og historikken er bevaret.";
                break;
            default:
                TempData["StatusMessage"] = "Produktet blev ikke fundet. Listen er opdateret.";
                break;
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateCategoriesAsync(
        AdminProductCreateViewModel model,
        CancellationToken cancellationToken)
    {
        var categories = await _products.GetAllCategoriesAsync(cancellationToken);
        model.Categories = categories.Select(category => new SelectListItem
        {
            Value = category.ProductCategoryId.ToString(),
            Text = category.Name,
            Selected = category.ProductCategoryId == model.CategoryId
        }).ToList();
    }

    private static async Task<(string? Extension, string? Error)> GetImageExtensionAsync(
        IFormFile? image,
        CancellationToken cancellationToken)
    {
        if (image is null || image.Length == 0)
        {
            return (null, "Vælg et billede.");
        }

        if (image.Length > MaximumImageBytes)
        {
            return (null, "Billedet må højst fylde 5 MB.");
        }

        var header = new byte[12];
        await using var stream = image.OpenReadStream();
        var bytesRead = await stream.ReadAsync(header.AsMemory(), cancellationToken);

        if (bytesRead >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
        {
            return (".jpg", null);
        }

        if (bytesRead >= 8
            && header.AsSpan(0, 8).SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }))
        {
            return (".png", null);
        }

        if (bytesRead >= 12
            && header.AsSpan(0, 4).SequenceEqual("RIFF"u8)
            && header.AsSpan(8, 4).SequenceEqual("WEBP"u8))
        {
            return (".webp", null);
        }

        return (null, "Brug et almindeligt JPG-, PNG- eller WebP-billede.");
    }

    private void AddCreationError(ProductCreateStatus status)
    {
        var (key, message) = status switch
        {
            ProductCreateStatus.CategoryNotFound =>
                (nameof(AdminProductCreateViewModel.CategoryId), "Den valgte kategori findes ikke længere. Vælg den igen."),
            ProductCreateStatus.CategoryAlreadyExists =>
                (nameof(AdminProductCreateViewModel.NewCategoryName), "Kategorien findes allerede. Vælg den i listen."),
            ProductCreateStatus.DuplicateProduct =>
                (nameof(AdminProductCreateViewModel.Model), "Et produkt med dette mærke og model findes allerede i kategorien."),
            ProductCreateStatus.InvalidRequest =>
                (string.Empty, "Kontrollér produktets kategori, varianter, dagspriser og antal."),
            _ => (string.Empty, "Produktet kunne ikke gemmes. Kontrollér oplysningerne, og prøv igen.")
        };

        ModelState.AddModelError(key, message);
    }

    private static void TryDeleteImage(string path)
    {
        try
        {
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
        }
        catch (IOException)
        {
            // Keep the original save error as the one shown by the application.
        }
        catch (UnauthorizedAccessException)
        {
            // Keep the original save error as the one shown by the application.
        }
    }

    private void TryDeleteProductImage(string? imageFileName)
    {
        if (string.IsNullOrWhiteSpace(imageFileName))
        {
            return;
        }

        var imageDirectory = Path.Combine(
            _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot"),
            "lib",
            "Images");
        var imagePath = Path.Combine(imageDirectory, Path.GetFileName(imageFileName));
        TryDeleteImage(imagePath);
    }
}
