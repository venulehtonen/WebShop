using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebShop.Services;

namespace WebShop.Pages.Admin;

[Authorize(Roles = "Admin")]
public class ProductsModel : PageModel
{
    private readonly IShopService _shopService;

    public ProductsModel(IShopService shopService)
    {
        _shopService = shopService;
    }

    [BindProperty]
    public ProductInput Input { get; set; } = new();

    [BindProperty]
    public int EditId { get; set; }

    public IReadOnlyList<ProductDto> Products { get; private set; } = [];
    public string StatusMessage { get; private set; } = string.Empty;

    public void OnGet()
    {
        LoadProducts();
    }

    public IActionResult OnPostCreate()
    {
        if (string.IsNullOrWhiteSpace(Input.Name) || Input.Price < 0)
        {
            StatusMessage = "Name is required and price must be 0 or more.";
            LoadProducts();
            return Page();
        }

        _shopService.AddProduct(new CreateProductRequest(Input.Name.Trim(), Input.Price, Input.Description?.Trim() ?? string.Empty));
        StatusMessage = "Product created.";
        Input = new ProductInput();
        EditId = 0;
        LoadProducts();
        return Page();
    }

    public IActionResult OnPostStartEdit(int productId)
    {
        var product = _shopService.GetProductById(productId);
        if (product is null)
        {
            StatusMessage = "Product not found.";
            LoadProducts();
            return Page();
        }

        EditId = product.Id;
        Input = new ProductInput
        {
            Name = product.Name,
            Price = product.Price,
            Description = product.Description
        };

        LoadProducts();
        return Page();
    }

    public IActionResult OnPostUpdate()
    {
        if (EditId <= 0)
        {
            StatusMessage = "No product selected for update.";
            LoadProducts();
            return Page();
        }

        if (string.IsNullOrWhiteSpace(Input.Name) || Input.Price < 0)
        {
            StatusMessage = "Name is required and price must be 0 or more.";
            LoadProducts();
            return Page();
        }

        var updated = _shopService.UpdateProduct(EditId, new UpdateProductRequest(Input.Name.Trim(), Input.Price, Input.Description?.Trim() ?? string.Empty));
        StatusMessage = updated ? "Product updated." : "Product not found.";

        if (updated)
        {
            EditId = 0;
            Input = new ProductInput();
        }

        LoadProducts();
        return Page();
    }

    public IActionResult OnPostDelete(int productId)
    {
        var deleted = _shopService.DeleteProduct(productId);
        StatusMessage = deleted ? "Product deleted." : "Product not found.";

        if (EditId == productId)
        {
            EditId = 0;
            Input = new ProductInput();
        }

        LoadProducts();
        return Page();
    }

    private void LoadProducts()
    {
        Products = _shopService.GetProducts();
    }

    public class ProductInput
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
