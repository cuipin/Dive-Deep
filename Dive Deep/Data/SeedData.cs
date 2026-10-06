using Dive_Deep.Models;
using Microsoft.EntityFrameworkCore;

namespace Dive_Deep.Data;

/// <summary>
/// Seeds the catalog after its EF migration has been applied.
/// Adds clearly tagged prototype inventory because the datasheet does not include stock counts.
/// </summary>
public static class SeedData
{
    private static readonly string[] Sizes = ["XS", "S", "M", "L", "XL"];

    public static async Task SeedCatalogAsync(
        Dive_DeepContext db,
        CancellationToken cancellationToken = default)
    {
        var categories = await db.ProductCategories
            .ToDictionaryAsync(category => category.Name, cancellationToken);

        foreach (var categoryName in Catalog.Select(product => product.Category).Distinct())
        {
            if (!categories.ContainsKey(categoryName))
            {
                var category = new ProductCategory { Name = categoryName };
                db.ProductCategories.Add(category);
                categories.Add(categoryName, category);
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        var existingProducts = await db.Products
            .Include(product => product.Variants)
            .ToListAsync(cancellationToken);

        foreach (var definition in Catalog)
        {
            var product = existingProducts.SingleOrDefault(candidate =>
                candidate.ProductCategoryId == categories[definition.Category].ProductCategoryId
                && candidate.Brand == definition.Brand
                && candidate.Model == definition.Model);

            if (product is null)
            {
                product = new Product
                {
                    Category = categories[definition.Category],
                    Brand = definition.Brand,
                    Model = definition.Model,
                    ImageFileName = definition.ImageFileName,
                    IsActive = true
                };

                db.Products.Add(product);
                existingProducts.Add(product);
            }

            product.ImageFileName ??= definition.ImageFileName;

            foreach (var variant in definition.Variants)
            {
                var existingVariant = product.Variants.SingleOrDefault(candidate =>
                    string.Equals(candidate.OptionLabel, variant.OptionLabel, StringComparison.OrdinalIgnoreCase));
                if (existingVariant is not null)
                {
                    existingVariant.EquipmentType ??= variant.EquipmentType;
                    continue;
                }

                product.Variants.Add(new ProductVariant
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
                    IsActive = true
                });
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        await SeedPrototypeEquipmentAsync(db, cancellationToken);
    }

    private static async Task SeedPrototypeEquipmentAsync(
        Dive_DeepContext db,
        CancellationToken cancellationToken)
    {
        const int demoUnitsPerVariant = 3;
        var variants = await db.ProductVariants
            .Include(variant => variant.EquipmentUnits)
            .Where(variant => variant.IsActive && variant.Product.IsActive)
            .ToListAsync(cancellationToken);
        var knownAssetTags = variants
            .SelectMany(variant => variant.EquipmentUnits)
            .Select(unit => unit.AssetTag)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var variant in variants)
        {
            for (var unitNumber = 1; unitNumber <= demoUnitsPerVariant; unitNumber++)
            {
                var assetTag = $"DEMO-{variant.ProductVariantId:D4}-{unitNumber:D2}";
                if (!knownAssetTags.Add(assetTag))
                {
                    continue;
                }

                db.EquipmentUnits.Add(new EquipmentUnit
                {
                    ProductVariantId = variant.ProductVariantId,
                    AssetTag = assetTag,
                    Status = EquipmentUnitStatus.Active
                });
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static readonly IReadOnlyList<CatalogProductSeed> Catalog = BuildCatalog();

    private static IReadOnlyList<CatalogProductSeed> BuildCatalog()
    {
        var products = new List<CatalogProductSeed>();

        AddSizedProduct(products, "BCD", "Scubapro", "Navigator Lite BCD", 125m, "BCD.jpg", ["S", "M", "L"]);
        AddSizedProduct(products, "BCD", "Scubapro", "BCD Glide", 140m, "BCD.jpg", ["S", "M", "L"]);
        AddSizedProduct(products, "BCD", "Scubapro", "BCD Hydros Pro", 200m, "BCD.jpg", ["S", "M", "L"]);
        AddSizedProduct(products, "BCD", "Seac", "BCD Modular", 145m, "BCD.jpg", ["S", "M", "L"]);

        AddSuit(products, "Scubapro", "Definition", "Våddragt", [3m, 5m, 7m], 100m);
        AddSuit(products, "Waterproof", "W5", "Våddragt", [3.5m], 100m);
        AddSuit(products, "Fourth Element", "Proteus", "Våddragt", [5m], 120m);
        AddSuit(products, "Scubapro", "Exodry 4.0", "Tørdragt", [null], 300m);
        AddSuit(products, "Waterproof", "D7 Evo", "Tørdragt", [null], 320m);
        AddSuit(products, "Santi", "E.Lite Plus", "Tørdragt", [null], 350m);

        products.Add(new CatalogProductSeed(
            "Tanke",
            "Scubapro",
            "Tank",
            "Tank.webp",
            [
                Variant("5 L", 150m, volumeLiters: 5m),
                Variant("10 L", 160m, volumeLiters: 10m),
                Variant("12 L", 170m, volumeLiters: 12m),
                Variant("15 L", 180m, volumeLiters: 15m)
            ]));

        products.Add(new CatalogProductSeed(
            "Regulatorsæt",
            "Scubapro",
            "Regulatorsæt",
            "Regulatorsæt.WEBP",
            [
                Variant("MK25EVO / S600 / R105", 125m, firstStage: "MK25EVO", secondStage: "S600", octopus: "R105"),
                Variant("MK17EVO / C370 / R095", 100m, firstStage: "MK17EVO", secondStage: "C370", octopus: "R095"),
                Variant("MK25EVO BT / A700 Carbon BT / S270", 150m, firstStage: "MK25EVO BT", secondStage: "A700 Carbon BT", octopus: "S270")
            ]));

        AddStandardProduct(products, "Maske/Snorkel", "Scubapro", "Ghost", 50m);
        AddStandardProduct(products, "Maske/Snorkel", "Scubapro", "D-Mask", 60m);
        AddStandardProduct(products, "Maske/Snorkel", "Scubapro", "Spectra Mini", 50m);
        AddStandardProduct(products, "Maske/Snorkel", "Scubapro", "Crystal VU", 75m);
        AddStandardProduct(products, "Maske/Snorkel", "Fourth Element", "Scout Kontrast", 75m);
        AddStandardProduct(products, "Maske/Snorkel", "Fourth Element", "Scout Enhance", 75m);
        AddStandardProduct(products, "Maske/Snorkel", "Tusa", "Element", 75m);
        AddStandardProduct(products, "Maske/Snorkel", "Scubapro", "Spectra Snorkel", 25m, "Snorkel");
        AddStandardProduct(products, "Maske/Snorkel", "Tusa", "Hyperdry Snorkel", 30m, "Snorkel");

        AddSizedProduct(products, "Finner", "Scubapro", "Jet Fin", 50m, "Finner.jpg", Sizes);
        AddSizedProduct(products, "Finner", "Scubapro", "GO Travel", 50m, "Finner.jpg", Sizes);
        AddSizedProduct(products, "Finner", "Scubapro", "Seawing Supernova", 60m, "Finner.jpg", Sizes);
        AddSizedProduct(products, "Finner", "Seac", "Propulsion", 50m, "Finner.jpg", Sizes);
        AddSizedProduct(products, "Finner", "Seac", "ALA", 50m, "Finner.jpg", Sizes);
        AddSizedProduct(products, "Finner", "Fourth Element", "Tech", 75m, "Finner.jpg", Sizes);
        AddSizedProduct(products, "Finner", "Fourth Element", "Rec Fin", 80m, "Finner.jpg", Sizes);

        return products;
    }

    private static void AddSuit(
        ICollection<CatalogProductSeed> products,
        string brand,
        string model,
        string equipmentType,
        IReadOnlyList<decimal?> thicknessOptions,
        decimal dailyRate)
    {
        var variants = new List<VariantSeed>();
        foreach (var thickness in thicknessOptions)
        {
            foreach (var gender in new[] { "Herre", "Dame" })
            {
                foreach (var size in Sizes)
                {
                    var typeLabel = thickness is null
                        ? equipmentType
                        : $"{equipmentType} {thickness.Value:0.#} mm";
                    var label = $"{typeLabel} / {gender} / {size}";
                    variants.Add(Variant(
                        label,
                        dailyRate,
                        size: size,
                        gender: gender,
                        equipmentType: equipmentType,
                        thicknessMm: thickness));
                }
            }
        }

        products.Add(new CatalogProductSeed(
            "Dykkerdragter",
            brand,
            model,
            "Dykkerdragt.webp",
            variants));
    }

    private static void AddSizedProduct(
        ICollection<CatalogProductSeed> products,
        string category,
        string brand,
        string model,
        decimal dailyRate,
        string imageFileName,
        IReadOnlyList<string> sizes)
    {
        products.Add(new CatalogProductSeed(
            category,
            brand,
            model,
            imageFileName,
            sizes.Select(size => Variant($"Størrelse {size}", dailyRate, size: size)).ToArray()));
    }

    private static void AddStandardProduct(
        ICollection<CatalogProductSeed> products,
        string category,
        string brand,
        string model,
        decimal dailyRate,
        string equipmentType = "Maske")
    {
        products.Add(new CatalogProductSeed(
            category,
            brand,
            model,
            "Snorkel maske.webp",
            [Variant("Standard", dailyRate, equipmentType: equipmentType)]));
    }

    private static VariantSeed Variant(
        string optionLabel,
        decimal dailyRate,
        string? size = null,
        string? gender = null,
        string? equipmentType = null,
        decimal? thicknessMm = null,
        decimal? volumeLiters = null,
        string? firstStage = null,
        string? secondStage = null,
        string? octopus = null) =>
        new(optionLabel, dailyRate, size, gender, equipmentType, thicknessMm, volumeLiters,
            firstStage, secondStage, octopus);

    private sealed record CatalogProductSeed(
        string Category,
        string Brand,
        string Model,
        string ImageFileName,
        IReadOnlyList<VariantSeed> Variants);

    private sealed record VariantSeed(
        string OptionLabel,
        decimal DailyRate,
        string? Size,
        string? Gender,
        string? EquipmentType,
        decimal? ThicknessMm,
        decimal? VolumeLiters,
        string? FirstStage,
        string? SecondStage,
        string? Octopus);
}
