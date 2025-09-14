using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PosLite.Common;

namespace PosLite.Pages.Categories;

public class IndexModel : PageModel
{
    private readonly AppDb _db;
    public IndexModel(AppDb db) => _db = db;

    [BindProperty(SupportsGet = true)] 
    public string? q { get; set; }
    [BindProperty(SupportsGet = true)] 
    public string status { get; set; } = "all";
    [BindProperty(SupportsGet = true)] 
    public int pageIndex { get; set; } = 1;
    [BindProperty(SupportsGet = true)]
    public int pageSize { get; set; } = 10;

    public int TotalItems { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / Math.Max(1, pageSize));
    public List<Row> Items { get; set; } = new();

    public class Row
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public bool IsActive { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// Handle GET requests to display the list of categories with filtering and pagination.
    /// </summary>
    /// <returns></returns>
    public async Task OnGet()
    {
        var query = _db.Categories.IgnoreQueryFilters().AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = TextSearch.Normalize(q);
            query = query.Where(x => x.NameSearch.Contains(term));
        }

        if (status == "active") query = query.Where(x => x.IsActive);
        else if (status == "inactive") query = query.Where(x => !x.IsActive);

        TotalItems = await query.CountAsync();

        if (pageIndex < 1) pageIndex = 1;
        if (pageSize < 1) pageSize = 10;
        var skip = (pageIndex - 1) * pageSize;

        Items = await query
            .OrderBy(x => x.Name)
            .Skip(skip).Take(pageSize)
            .Select(x => new Row
            {
                Id = x.CategoryId,
                Name = x.Name,
                IsActive = x.IsActive,
                CreatedBy = x.CreatedBy,
                CreatedAt = x.CreatedAt,
                UpdatedBy = x.UpdatedBy,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync();
    }

    /// <summary>
    /// Handle POST requests to deactivate a category by its ID.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<IActionResult> OnPostDeactivateAsync(Guid id)
    {
        var cat = await _db.Categories.FindAsync(id);

        if (cat == null) return NotFound();

        cat.IsActive = false;
        await _db.SaveChangesAsync();

        TempData["Toast.Type"] = "success";
        TempData["Toast.Text"] = "Đã chuyển danh mục sang trạng thái ngừng sử dụng.";
        return RedirectToPage("./Index", new { q, status, pageIndex, pageSize });
    }

    /// <summary>
    /// Handle POST requests to activate a category by its ID.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<IActionResult> OnPostActivateAsync(Guid id)
    {
        var cat = await _db.Categories.IgnoreQueryFilters()
                                      .FirstOrDefaultAsync(x => x.CategoryId == id);
        if (cat == null) return NotFound();

        cat.IsActive = true;
        await _db.SaveChangesAsync();

        TempData["Toast.Type"] = "success";
        TempData["Toast.Text"] = "Đã chuyển danh mục sang trạng thái sử dụng.";
        return RedirectToPage("./Index", new { q, status, pageIndex, pageSize });
    }

    /// <summary>
    /// Handle POST requests to bulk delete categories by their IDs.
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    public async Task<IActionResult> OnPostBulkDeleteAsync([FromForm] List<Guid> ids)
    {
        if (ids is null || ids.Count == 0)
        {
            TempData["Toast.Type"] = "error";
            TempData["Toast.Text"] = "Chưa chọn danh mục nào.";
            return RedirectToPage("./Index", new { q, status, pageIndex, pageSize });
        }

        var blockedIds = (await _db.Products
            .Where(p => p.CategoryId != null && ids.Contains(p.CategoryId.Value))
            .Select(p => p.CategoryId!.Value)
            .Distinct()
            .ToListAsync()).ToHashSet();
        var okIds = ids.Where(id => !blockedIds.Contains(id)).ToList();

        if (okIds.Count > 0)
        {
            await _db.Categories
                .Where(c => okIds.Contains(c.CategoryId))
                .ExecuteDeleteAsync(); 
        }

        if (blockedIds.Count > 0)
        {
            var blockedNames = await _db.Categories
                .IgnoreQueryFilters()
                .Where(c => blockedIds.Contains(c.CategoryId))
                .Select(c => c.Name)
                .ToListAsync();

            TempData["Toast.Type"] = okIds.Count > 0 ? "error" : "error";
            TempData["Toast.Text"] = $"Không thể xóa: {string.Join(", ", blockedNames)} vì đang có sản phẩm liên kết.";
        }
        else
        {
            TempData["Toast.Type"] = "success";
            TempData["Toast.Text"] = $"Đã xóa {okIds.Count} danh mục.";
        }

        var afterQuery = _db.Categories.AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = TextSearch.Normalize(q);
            afterQuery = afterQuery.Where(x => x.NameSearch.Contains(term));
        }

        var totalAfter = await afterQuery.CountAsync();
        var totalPagesAfter = Math.Max(1, (int)Math.Ceiling((double)totalAfter / Math.Max(1, pageSize)));
        var newPageIndex = Math.Min(pageIndex, totalPagesAfter);  

        return RedirectToPage("./Index", new { q, status, pageIndex = newPageIndex, pageSize });
    }
}
