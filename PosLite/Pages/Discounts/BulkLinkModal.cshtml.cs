using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace PosLite.Pages.Discounts;

public class BulkLinkModalModel : PageModel
{
    private readonly AppDb _db;
    public BulkLinkModalModel(AppDb db) => _db = db;

    [BindProperty] public InputModel M { get; set; } = new();

    public List<(Guid Id, string Name)> Products { get; set; } = new();
    public List<(Guid Id, string Name)> Customers { get; set; } = new();

    public class InputModel
    {
        [Required(ErrorMessage = "Vui lòng chọn sản phẩm")] public Guid? ProductId { get; set; }
        [MinLength(1, ErrorMessage = "Vui lòng chọn ít nhất 1 khách hàng")]
        public List<Guid> CustomerIds { get; set; } = new();
        [Range(0, 100, ErrorMessage = "0–100%")] public double Percent { get; set; }
    }

    public async Task OnGet()
    {
        Products = await _db.Products.OrderBy(x => x.Name).Select(x => new ValueTuple<Guid, string>(x.ProductId, x.Name)).ToListAsync();
        Customers = await _db.Customers.OrderBy(x => x.Name).Select(x => new ValueTuple<Guid, string>(x.CustomerId, x.Name)).ToListAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await OnGet();
            return Page();
        }

        var pid = M.ProductId!.Value;

        var existing = await _db.CustomerProductDiscounts
            .IgnoreQueryFilters()
            .Where(x => x.ProductId == pid && M.CustomerIds.Contains(x.CustomerId))
            .ToListAsync();
        var existMap = existing.ToDictionary(x => x.CustomerId, x => x);

        foreach (var cid in M.CustomerIds.Distinct())
        {
            if (existMap.TryGetValue(cid, out var row))
            {
                row.Percent = M.Percent;
                row.IsActive = true;
            }
            else
            {
                _db.CustomerProductDiscounts.Add(new CustomerProductDiscount
                {
                    Id = Guid.NewGuid(),
                    ProductId = pid,
                    CustomerId = cid,
                    Percent = M.Percent,
                    IsActive = true
                });
            }
        }

        await _db.SaveChangesAsync();

        TempData["Toast.Type"] = "success";
        TempData["Toast.Text"] = "Đã liên kết khách hàng thành công.";

        return Content(@"<script>
            window.appToast?.ok('Đã liên kết khách hàng thành công.');
            const el = document.getElementById('bulkModal');
            if (el) bootstrap.Modal.getInstance(el)?.hide();
            window.location.reload();
        </script>", "text/html");
    }
}
