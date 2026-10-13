using Dive_Deep.DTOs;
using Dive_Deep.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace Dive_Deep.Controllers
{
    [Route("api/categories")]
    [ApiController]
    public class ApiCategoriesController : ControllerBase
    {
        private readonly IProductRepository _productRepository;

        public ApiCategoriesController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        // GET: api/categories
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var categories = await _productRepository.GetAllCategoriesAsync();
            return Ok(categories.Select(category => category.ToDto()));
        }
    }
}