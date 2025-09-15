using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace PosLite.Pages.Discounts;

public class IndexModel : PageModel
{
    private readonly AppDb _db;
    public IndexModel(AppDb db) => _db = db;

    [BindProperty(SupportsGet = true)] public string? q { get; set; }
    [BindProperty(SupportsGet = true)] public string status { get; set; } = "all";
    [BindProperty(SupportsGet = true)] public double? percent { get; set; }
    [BindProperty(SupportsGet = true)] public int pageIndex { get; set; } = 1;
    [BindProperty(SupportsGet = true)] public int pageSize { get; set; } = 10;

    public int TotalItems { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / Math.Max(1, pageSize));

    public List<Row> Items { get; set; } = new();
    public List<double> DistinctPercents { get; set; } = new();

    public class Row
    {
        public Guid Id { get; set; }
        public string ProductCode { get; set; } = "";
        public string ProductName { get; set; } = "";
        public string CustomerName { get; set; } = "";
        public double Percent { get; set; }
        public bool IsActive { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public async Task OnGet()
    {
        DistinctPercents = await _db.CustomerProductDiscounts
            .Select(x => x.Percent).Distinct().OrderBy(x => x).ToListAsync();

        var query =
            from d in _db.CustomerProductDiscounts
            join p in _db.Products on d.ProductId equals p.ProductId
            join c in _db.Customers on d.CustomerId equals c.CustomerId
            select new { d, p, c };

        if (!string.IsNullOrWhiteSpace(q))
        {
            var t = PosLite.Common.TextSearch.Normalize(q);
            query = query.Where(x =>
                x.p.NameSearch.Contains(t) || x.p.CodeSearch.Contains(t) ||
                (x.c.Name != null && x.c.Name.ToLower().Contains(q.ToLower())));
        }

        if (status == "active") query = query.Where(x => x.d.IsActive);
        else if (status == "inactive") query = query.Where(x => !x.d.IsActive);

        if (percent.HasValue) query = query.Where(x => x.d.Percent == percent.Value);

        TotalItems = await query.CountAsync();
        if (pageIndex < 1) pageIndex = 1;
        if (pageSize < 1) pageSize = 10;
        var skip = (pageIndex - 1) * pageSize;

        Items = await query.OrderBy(x => x.p.Name).ThenBy(x => x.c.Name)
            .Skip(skip).Take(pageSize)
            .Select(x => new Row
            {
                Id = x.d.Id,
                ProductCode = x.p.Code,
                ProductName = x.p.Name,
                CustomerName = x.c.Name,
                Percent = x.d.Percent,
                IsActive = x.d.IsActive,
                CreatedBy = x.d.CreatedBy,
                CreatedAt = x.d.CreatedAt,
                UpdatedBy = x.d.UpdatedBy,
                UpdatedAt = x.d.UpdatedAt
            }).ToListAsync();
    }

    public async Task<IActionResult> OnPostActivateAsync(Guid id)
    {
        var e = await _db.CustomerProductDiscounts.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id);
        if (e == null) return NotFound();
        e.IsActive = true;
        await _db.SaveChangesAsync();
        TempData["Toast.Type"] = "success";
        TempData["Toast.Text"] = "Đã bật liên kết.";
        return RedirectToPage("./Index", new { q, status, percent, pageIndex, pageSize });
    }

    public async Task<IActionResult> OnPostDeactivateAsync(Guid id)
    {
        var e = await _db.CustomerProductDiscounts.FindAsync(id);
        if (e == null) return NotFound();
        e.IsActive = false;
        await _db.SaveChangesAsync();
        TempData["Toast.Type"] = "success";
        TempData["Toast.Text"] = "Đã tắt liên kết.";
        return RedirectToPage("./Index", new { q, status, percent, pageIndex, pageSize });
    }

    public async Task<IActionResult> OnPostBulkDeleteAsync([FromForm] List<Guid> ids)
    {
        if (ids == null || ids.Count == 0)
        {
            TempData["Toast.Type"] = "error";
            TempData["Toast.Text"] = "Chưa chọn liên kết nào.";
            return RedirectToPage("./Index", new { q, status, percent, pageIndex, pageSize });
        }

        await _db.CustomerProductDiscounts.Where(x => ids.Contains(x.Id)).ExecuteDeleteAsync();

        TempData["Toast.Type"] = "success";
        TempData["Toast.Text"] = $"Đã xóa {ids.Count} liên kết.";

        var totalAfter = await _db.CustomerProductDiscounts.CountAsync();
        var totalPagesAfter = Math.Max(1, (int)Math.Ceiling(totalAfter / (double)Math.Max(1, pageSize)));
        var newPageIndex = Math.Min(pageIndex, totalPagesAfter);

        return RedirectToPage("./Index", new { q, status, percent, pageIndex = newPageIndex, pageSize });
    }
}
