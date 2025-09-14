using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PosLite.Common;

namespace PosLite.Pages.Products;

public class IndexModel : PageModel
{
    private readonly AppDb _db;
    public IndexModel(AppDb db) => _db = db;

    [BindProperty(SupportsGet = true)] public string? q { get; set; }
    [BindProperty(SupportsGet = true)] public string status { get; set; } = "all";
    [BindProperty(SupportsGet = true)] public Guid? categoryId { get; set; }
    [BindProperty(SupportsGet = true)] public int pageIndex { get; set; } = 1;
    [BindProperty(SupportsGet = true)] public int pageSize { get; set; } = 10;

    public int TotalItems { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / Math.Max(1, pageSize));

    public List<Row> Items { get; set; } = new();
    public List<CatItem> Categories { get; set; } = new();

    public record CatItem(Guid Id, string Name);
    public class Row
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string Unit { get; set; } = "";
        public string? CategoryName { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// Handle GET requests to display the list of products with filtering and pagination.
    /// </summary>
    /// <returns></returns>
    public async Task OnGet()
    {
        Categories = await _db.Categories
            .OrderBy(x => x.Name).Select(x => new CatItem(x.CategoryId, x.Name)).ToListAsync();

        var query = _db.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = TextSearch.Normalize(q);
            query = query.Where(p => p.NameSearch.Contains(term) || p.CodeSearch.Contains(term));
        }

        if (status == "active") query = query.Where(p => p.IsActive);
        else if (status == "inactive") query = query.Where(p => !p.IsActive);

        if (categoryId.HasValue) query = query.Where(p => p.CategoryId == categoryId.Value);

        TotalItems = await query.CountAsync();
        if (pageIndex < 1) pageIndex = 1;
        if (pageSize < 1) pageSize = 10;
        var skip = (pageIndex - 1) * pageSize;

        Items = await query
            .OrderBy(p => p.Name)
            .Skip(skip).Take(pageSize)
            .Select(p => new Row
            {
                Id = p.ProductId,
                Code = p.Code,
                Name = p.Name,
                Unit = p.Unit,
                CategoryName = _db.Categories.Where(c => c.CategoryId == p.CategoryId).Select(c => c.Name).FirstOrDefault(),
                Price = p.Price,
                IsActive = p.IsActive,
                CreatedBy = p.CreatedBy,
                CreatedAt = p.CreatedAt,
                UpdatedBy = p.UpdatedBy,
                UpdatedAt = p.UpdatedAt
            }).ToListAsync();
    }

    /// <summary>
    /// Handle POST requests to activate a product by its ID.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<IActionResult> OnPostActivateAsync(Guid id)
    {
        var p = await _db.Products.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.ProductId == id);

        if (p == null) return NotFound();

        p.IsActive = true;
        await _db.SaveChangesAsync();
        TempData["Toast.Type"] = "success";
        TempData["Toast.Text"] = "Đã chuyển sản phẩm sang trạng thái sử dụng.";
        return RedirectToPage("./Index", new { q, status, categoryId, pageIndex, pageSize });
    }

    /// <summary>
    /// Handle POST requests to deactivate a product by its ID.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<IActionResult> OnPostDeactivateAsync(Guid id)
    {
        var p = await _db.Products.FindAsync(id);
        if (p == null) return NotFound();
        p.IsActive = false;
        await _db.SaveChangesAsync();
        TempData["Toast.Type"] = "success";
        TempData["Toast.Text"] = "Đã chuyển sản phẩm sang trạng thái ngừng sử dụng.";
        return RedirectToPage("./Index", new { q, status, categoryId, pageIndex, pageSize });
    }

    /// <summary>
    /// Handle POST requests to bulk delete products by their IDs.
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    public async Task<IActionResult> OnPostBulkDeleteAsync([FromForm] List<Guid> ids)
    {
        if (ids is null || ids.Count == 0)
        {
            TempData["Toast.Type"] = "error";
            TempData["Toast.Text"] = "Chưa chọn sản phẩm nào.";
            return RedirectToPage("./Index", new { q, status, categoryId, pageIndex, pageSize });
        }

        var blocked = (await _db.SaleInvoiceLines
                .Where(l => ids.Contains(l.ProductId))
                .Select(l => l.ProductId)
                .Distinct()
                .ToListAsync())
            .ToHashSet();

        var okIds = ids.Where(id => !blocked.Contains(id)).ToList();

        if (okIds.Count > 0)
        {
            await _db.Products.Where(p => okIds.Contains(p.ProductId)).ExecuteDeleteAsync();
        }

        if (blocked.Count > 0)
        {
            var names = await _db.Products.IgnoreQueryFilters()
                            .Where(p => blocked.Contains(p.ProductId))
                            .Select(p => p.Name)
                            .ToListAsync();

            TempData["Toast.Type"] = "error";
            TempData["Toast.Text"] = $"Không thể xóa: {string.Join(", ", names)} vì đang có hóa đơn sử dụng.";
        }
        else
        {
            TempData["Toast.Type"] = "success";
            TempData["Toast.Text"] = $"Đã xóa {okIds.Count} sản phẩm.";
        }

        var after = _db.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = TextSearch.Normalize(q);
            after = after.Where(p => p.NameSearch.Contains(term) || p.CodeSearch.Contains(term));
        }

        if (categoryId.HasValue) after = after.Where(p => p.CategoryId == categoryId);

        var totalAfter = await after.CountAsync();
        var totalPagesAfter = Math.Max(1, (int)Math.Ceiling(totalAfter / (double)Math.Max(1, pageSize)));
        var newPage = Math.Min(pageIndex, totalPagesAfter);

        return RedirectToPage("./Index", new { q, status, categoryId, pageIndex = newPage, pageSize });
    }
}
