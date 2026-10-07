using Dive_Deep.DTOs;
using Dive_Deep.Models;
using Dive_Deep.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dive_Deep.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ApiProductsController : ControllerBase
    {
        private readonly IProductRepository _productRepository;

        public ApiProductsController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        // GET: api/products
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var products = await _productRepository.GetAllProductsAsync();
            return Ok(products.Select(product => product.ToDto()));
        }

        // ROUTE: GET api/products/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetProductById([FromRoute] int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            var product = await _productRepository.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            return Ok(product.ToDto());
        }

        // QUERY STRING: GET api/products/search?search=X&category=X
        [HttpGet("search")]
        public async Task<IActionResult> Search(
            [FromQuery] string? search,
            [FromQuery] string? category)
        {
            var products = await _productRepository.SearchProductsAsync(search, category, null, null);
            return Ok(products.Select(product => product.ToDto()));
        }

        // HEADER: GET api/products/by-category  (header: X-Category: X)
        [HttpGet("by-category")]
        public async Task<IActionResult> GetByCategory([FromHeader(Name = "X-Category")] string category)
        {
            var products = await _productRepository.GetByCategoryNameAsync(category);
            return Ok(products.Select(product => product.ToDto()));
        }

        // BODY: POST api/products
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public Task<IActionResult> CreateProduct([FromBody] ProductDto dto) => CreateAsync(dto);

        // FORM: POST api/products/form 
        [Authorize(Roles = "Admin")]
        [HttpPost("form")]
        public Task<IActionResult> CreateProductFromForm([FromForm] ProductDto dto) => CreateAsync(dto);

        // PUT: api/products/5
        // PUT: api/products/5
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateProduct([FromRoute] int id, [FromBody] ProductDto dto)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingProduct = await _productRepository.GetProductByIdAsync(id);
            if (existingProduct == null)
            {
                return NotFound();
            }

            if (!await _productRepository.CategoryExistsAsync(dto.ProductCategoryId))
            {
                return BadRequest("Kategori findes ikke.");
            }

            var product = dto.ToEntity();
            product.ProductId = id;

            var updated = await _productRepository.UpdateProductAsync(product);
            if (!updated)
            {
                return Conflict("Et produkt med samme mærke og model findes allerede i kategorien.");
            }

            return NoContent();
        }

        // DELETE: api/products/5
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteProduct([FromRoute] int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            var outcome = await _productRepository.DeleteProductAsAdminAsync(id);

            return outcome.Status switch
            {
                ProductDeletionStatus.NotFound => NotFound(),
                ProductDeletionStatus.Archived => Ok("Produktet er arkiveret, fordi det bruges i bookinger eller kurve."),
                _ => NoContent()
            };
        }

        private async Task<IActionResult> CreateAsync(ProductDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            dto.Id = 0;

            if (!await _productRepository.CategoryExistsAsync(dto.ProductCategoryId))
            {
                return BadRequest("Kategori findes ikke.");
            }

            var created = await _productRepository.AddProductAsAdminAsync(dto.ToEntity());
            if (created == null)
            {
                return Conflict("Et produkt med samme mærke og model findes allerede i kategorien.");
            }

            return CreatedAtAction(nameof(GetProductById), new { id = created.ProductId }, created.ToDto());
        }

        // GET: api/products/5/variants
        [HttpGet("{productId:int}/variants")]
        public async Task<IActionResult> GetProductVariants([FromRoute] int productId)
        {
            if (productId <= 0)
            {
                return BadRequest();
            }
            var variants = await _productRepository.GetProductVariantsAsync(productId);
            if (variants == null || !variants.Any())
            {
                return NotFound();
            }
            return Ok(variants.Select(variant => variant.ToDto()));
        }

    }
}