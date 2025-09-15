using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PosLite.Common;

namespace PosLite.Pages.Customers;

public class IndexModel : PageModel
{
    private readonly AppDb _db;
    public IndexModel(AppDb db) => _db = db;

    [BindProperty(SupportsGet = true)] public string? q { get; set; }
    [BindProperty(SupportsGet = true)] public string status { get; set; } = "all";
    [BindProperty(SupportsGet = true)] public int pageIndex { get; set; } = 1;
    [BindProperty(SupportsGet = true)] public int pageSize { get; set; } = 10;

    public int TotalItems { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / Math.Max(1, pageSize));

    public List<Row> Items { get; set; } = new();

    public class Row
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int Balance { get; set; }
    }

    public async Task OnGet()
    {
        var query = _db.Customers.IgnoreQueryFilters().AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = TextSearch.Normalize(q);
            query = query.Where(x =>
                x.CodeSearch.Contains(term) || x.NameSearch.Contains(term) ||
                (x.Phone != null && x.Phone.Contains(term)) ||
                (x.Address != null && x.Address.Contains(term)));
        }

        if (status == "active") query = query.Where(x => x.IsActive);
        else if (status == "inactive") query = query.Where(x => !x.IsActive);

        TotalItems = await query.CountAsync();

        if (pageIndex < 1) pageIndex = 1;
        if (pageSize < 1) pageSize = 10;
        var skip = (pageIndex - 1) * pageSize;

        var page = await query
            .OrderBy(x => x.Name)
            .Skip(skip).Take(pageSize)
            .Select(x => new Row
            {
                Id = x.CustomerId,
                Code = x.Code,
                Name = x.Name,
                Phone = x.Phone,
                Address = x.Address,
                IsActive = x.IsActive,
                CreatedBy = x.CreatedBy,
                CreatedAt = x.CreatedAt,
                UpdatedBy = x.UpdatedBy,
                UpdatedAt = x.UpdatedAt
            }).ToListAsync();

        var ids = page.Select(x => x.Id).ToList();
        var balances = await _db.CustomerLedgers
            .Where(l => ids.Contains(l.CustomerId))
            .GroupBy(l => l.CustomerId)
            .Select(g => new { Id = g.Key, Bal = g.Sum(x => x.Debit - x.Credit) })
            .ToDictionaryAsync(x => x.Id, x => x.Bal);

        foreach (var r in page)
            r.Balance = balances.TryGetValue(r.Id, out var b) ? b : 0;

        Items = page;
    }

    public async Task<IActionResult> OnPostActivateAsync(Guid id)
    {
        var c = await _db.Customers.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.CustomerId == id);
        if (c == null) return NotFound();
        c.IsActive = true;
        await _db.SaveChangesAsync();
        TempData["Toast.Type"] = "success";
        TempData["Toast.Text"] = "Đã chuyển khách hàng sang trạng thái sử dụng.";
        return RedirectToPage("./Index", new { q, status, pageIndex, pageSize });
    }

    public async Task<IActionResult> OnPostDeactivateAsync(Guid id)
    {
        var c = await _db.Customers.FirstOrDefaultAsync(x => x.CustomerId == id);
        if (c == null) return NotFound();
        c.IsActive = false;
        await _db.SaveChangesAsync();
        TempData["Toast.Type"] = "success";
        TempData["Toast.Text"] = "Đã chuyển khách hàng sang trạng thái ngừng sử dụng.";
        return RedirectToPage("./Index", new { q, status, pageIndex, pageSize });
    }

    public async Task<IActionResult> OnPostBulkDeleteAsync([FromForm] List<Guid> ids)
    {
        if (ids is null || ids.Count == 0)
        {
            TempData["Toast.Type"] = "error";
            TempData["Toast.Text"] = "Chưa chọn khách hàng nào.";
            return RedirectToPage("./Index", new { q, status, pageIndex, pageSize });
        }

        var blocked = new HashSet<Guid>(
            (await _db.SaleInvoices.Where(i => ids.Contains(i.CustomerId))
                .Select(i => i.CustomerId).Distinct().ToListAsync())
        );

        var okIds = ids.Where(id => !blocked.Contains(id)).ToList();

        if (okIds.Count > 0)
        {
            await _db.CustomerProductDiscounts
                .Where(d => okIds.Contains(d.CustomerId))
                .ExecuteDeleteAsync();

            await _db.Customers
                .Where(c => okIds.Contains(c.CustomerId))
                .ExecuteDeleteAsync();
        }

        if (blocked.Count > 0)
        {
            var names = await _db.Customers.IgnoreQueryFilters()
                .Where(c => blocked.Contains(c.CustomerId))
                .Select(c => c.Name)
                .ToListAsync();

            TempData["Toast.Type"] = "error";
            TempData["Toast.Text"] = "Không thể xóa: " +
                                     string.Join(", ", names) +
                                     " (đã phát sinh hóa đơn/bút toán).";
        }
        else
        {
            TempData["Toast.Type"] = "success";
            TempData["Toast.Text"] = $"Đã xóa {okIds.Count} khách hàng.";
        }

        // Tính lại trang hiện tại sau khi xóa (tránh rơi vào trang trống)
        var after = _db.Customers.IgnoreQueryFilters().AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = TextSearch.Normalize(q);
            after = after.Where(x =>
                x.CodeSearch.Contains(term) || x.NameSearch.Contains(term) ||
                (x.Phone != null && x.Phone.Contains(term)) ||
                (x.Address != null && x.Address.Contains(term)));
        }

        if (status == "active") after = after.Where(x => x.IsActive);
        else if (status == "inactive") after = after.Where(x => !x.IsActive);

        var totalAfter = await after.CountAsync();
        var totalPagesAfter = Math.Max(1, (int)Math.Ceiling(totalAfter / (double)Math.Max(1, pageSize)));
        var newPage = Math.Min(pageIndex, totalPagesAfter);

        return RedirectToPage("./Index", new { q, status, pageIndex = newPage, pageSize });
    }

}
