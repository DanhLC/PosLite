using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace PosLite.Pages.Products;

public class FormModalModel : PageModel
{
    private readonly AppDb _db;
    public FormModalModel(AppDb db) => _db = db;

    [BindProperty(SupportsGet = true)] public Guid? Id { get; set; }
    [BindProperty] public InputModel M { get; set; } = new();

    public bool IsEdit => Id.HasValue;
    public List<(Guid Id, string Name)> Categories { get; set; } = new();
    private static string GenerateProductCode()
        => "SP" + Guid.NewGuid().ToString("N").ToUpperInvariant();

    public class InputModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mã sản phẩm")]
        public string Code { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
        [StringLength(200, ErrorMessage = "Tên tối đa 200 ký tự")]
        public string Name { get; set; } = "";

        public string? Unit { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        public Guid? CategoryId { get; set; }

        public decimal Price { get; set; }=0;

        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Load data for edit
    /// </summary>
    /// <returns></returns>
    public async Task OnGetAsync()
    {
        Categories = await _db.Categories
            .OrderBy(x => x.Name)
            .Select(x => new ValueTuple<Guid, string>(x.CategoryId, x.Name))
            .ToListAsync();

        if (Id.HasValue)
        {
            var p = await _db.Products.IgnoreQueryFilters()
                                      .FirstOrDefaultAsync(x => x.ProductId == Id.Value);
            if (p != null)
            {
                M = new InputModel
                {
                    Code = p.Code,
                    Name = p.Name,
                    Unit = p.Unit,
                    CategoryId = p.CategoryId,
                    Price = p.Price,
                    IsActive = p.IsActive
                };
            }
        }
        else
        {
            M.Code = GenerateProductCode();
        }
    }

    /// <summary>
    /// Save (create or update)
    /// </summary>
    /// <returns></returns>
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        var exists = await _db.Products.IgnoreQueryFilters()
            .AnyAsync(x => x.Code == M.Code.Trim() && x.ProductId != Id);

        if (exists)
        {
            ModelState.AddModelError("M.Code", "Mã sản phẩm đã tồn tại.");
            await OnGetAsync();
            return Page();
        }

        if (Id == null) 
        {
            var p = new Product
            {
                ProductId = Guid.NewGuid(),
                Code = M.Code.Trim(),
                Name = M.Name.Trim(),
                Unit = M.Unit?.Trim() ?? "",
                CategoryId = M.CategoryId,
                Price = M.Price,
                IsActive = M.IsActive
            };
            _db.Products.Add(p);
        }
        else
        {
            var p = await _db.Products.IgnoreQueryFilters()
                                      .FirstOrDefaultAsync(x => x.ProductId == Id.Value);
            if (p == null) return NotFound();

            p.Code = M.Code.Trim();
            p.Name = M.Name.Trim();
            p.Unit = M.Unit?.Trim() ?? "";
            p.CategoryId = M.CategoryId;
            p.Price = M.Price;
            p.IsActive = M.IsActive;
        }

        await _db.SaveChangesAsync();

        if (Request.Headers.ContainsKey("HX-Request"))
        {
            var html = @"<script>
                window.appToast?.ok('Lưu sản phẩm thành công.');
                    const el = document.getElementById('catModal');
                    if (el) bootstrap.Modal.getInstance(el)?.hide();
                    window.location.reload();
                </script>";
            return Content(html, "text/html");
        }

        TempData["Toast.Type"] = "success";
        TempData["Toast.Text"] = "Lưu sản phẩm thành công.";
        return RedirectToPage("./Index");
    }
}
