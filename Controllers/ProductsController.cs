using System.Net;
using dotnet_hero.Data;
using dotnet_hero.DTOs.Product;
using dotnet_hero.Entities;
using dotnet_hero.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dotnet_hero.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(Roles = "Admin, Cashier")]
public class ProductsController : ControllerBase
{
    private readonly IProductService productService;
    public ProductsController(IProductService productService)
    {
        this.productService = productService;
    }

    [HttpGet] // https://localhost:5001/products
    public async Task<ActionResult<IEnumerable<ProductResponse>>> GetProducts()
    {
        return (await productService.FindAll())
            .Select(ProductResponse.FromProduct).ToList();
    }

    [HttpGet("{id}")] // https://localhost:5001/products/{id}
    public async Task<ActionResult<ProductResponse>> GetProductById(int id)
    {
        var product = await productService.FindById(id);

        if (product == null)
        {
            return NotFound();
        }

        return product.Adapt<ProductResponse>();
    }

    [HttpGet("search")] // https://localhost:5001/products/search?name=imac
    public async Task<ActionResult<IEnumerable<ProductResponse>>> SearchProducts([FromQuery] string? name = "")
    {
        return (await productService.Search(name))
                .Select(ProductResponse.FromProduct)
                .ToList();
    }

    [HttpPost] // https://localhost:5001/products
    public async Task<ActionResult<Product>> AddProducts([FromForm] ProductRequest productRequest)
    {
        (string errorMessage, string imageName) = await productService.UploadImage(productRequest.FormFiles);
        if (!String.IsNullOrEmpty(errorMessage)) return BadRequest();
        var product = productRequest.Adapt<Product>();
        product.Image = imageName;
        await productService.Create(product);
        return StatusCode((int)HttpStatusCode.Created);
    }

    [HttpPut("{id}")] // https://localhost:5001/products/{id}
    public async Task<ActionResult<Product>> UpdateProduct(int id, [FromForm] ProductRequest productRequest)
    {
        if (id != productRequest.ProductId)
        {
            return BadRequest();
        }

        var product = await productService.FindById(id);

        if (product == null)
        {
            return NotFound();
        }

        (string errorMessage, string imageName) = await productService.UploadImage(productRequest.FormFiles);
        if (!String.IsNullOrEmpty(errorMessage)) return BadRequest();

        if (!String.IsNullOrEmpty(imageName)) { product.Image = imageName; }

        productRequest.Adapt(product);
        await productService.Update(product);

        return NoContent();
    }

    [HttpDelete("{id}")] // https://localhost:5001/products/{id}
    public async Task<ActionResult> DeleteProductById(int id)
    {
        var product = await productService.FindById(id);

        if (product == null)
        {
            return NotFound();
        }

        await productService.Delete(product);

        return NoContent();
    }

}
