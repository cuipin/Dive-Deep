using Dive_Deep.Models;

namespace Dive_Deep.ViewModels;

public class HomePageViewModel
{
    public IReadOnlyList<Product> FeaturedProducts { get; init; } = Array.Empty<Product>();
}
