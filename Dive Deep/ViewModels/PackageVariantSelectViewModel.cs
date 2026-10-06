using Dive_Deep.Models;

namespace Dive_Deep.ViewModels;

public sealed record PackageVariantSelectViewModel(
    string Id,
    string Label,
    IReadOnlyList<ProductVariant> Variants);
