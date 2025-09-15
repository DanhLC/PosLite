using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace PosLite.Pages.Discounts;

public class FormModalModel : PageModel
{
    private readonly AppDb _db;
    public FormModalModel(AppDb db) => _db = db;

    [BindProperty(SupportsGet = true)] public Guid? Id { get; set; }
    [BindProperty] public InputModel M { get; set; } = new();

    public List<(Guid Id, string Name)> Products { get; set; } = new();
    public List<(Guid Id, string Name)> Customers { get; set; } = new();

    public class InputModel
    {
        [Required(ErrorMessage = "Vui lòng chọn sản phẩm")] public Guid? ProductId { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn khách hàng áp dụng")] public Guid? CustomerId { get; set; }
        [Range(0, 100, ErrorMessage = "0–100%")] public double Percent { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public async Task OnGet()
    {
        Products = await _db.Products.OrderBy(x => x.Name).Select(x => new ValueTuple<Guid, string>(x.ProductId, x.Name)).ToListAsync();
        Customers = await _db.Customers.OrderBy(x => x.Name).Select(x => new ValueTuple<Guid, string>(x.CustomerId, x.Name)).ToListAsync();

        if (Id.HasValue)
        {
            var e = await _db.CustomerProductDiscounts.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == Id.Value);
            if (e != null)
            {
                M = new InputModel
                {
                    ProductId = e.ProductId,
                    CustomerId = e.CustomerId,
                    Percent = e.Percent,
                    IsActive = e.IsActive
                };
            }
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) { await OnGet(); return Page(); }

        // Quy tắc: (CustomerId, ProductId) là duy nhất → nếu đã có thì cập nhật % và ngày
        var exist = await _db.CustomerProductDiscounts
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.CustomerId == M.CustomerId && x.ProductId == M.ProductId);

        if (Id == null)
        {
            if (exist != null)
            {
                exist.Percent = M.Percent;
                exist.IsActive = M.IsActive;
            }
            else
            {
                _db.CustomerProductDiscounts.Add(new CustomerProductDiscount
                {
                    Id = Guid.NewGuid(),
                    CustomerId = M.CustomerId!.Value,
                    ProductId = M.ProductId!.Value,
                    Percent = M.Percent,
                    IsActive = M.IsActive
                });
            }
        }
        else
        {
            var row = await _db.CustomerProductDiscounts.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == Id.Value);
            if (row == null) return NotFound();

            // Nếu đổi sang KH/SP trùng bản khác → merge (theo quy tắc duy nhất)
            if (exist != null && exist.Id != row.Id)
            {
                exist.Percent = M.Percent;
                exist.IsActive = M.IsActive;
                _db.CustomerProductDiscounts.Remove(row);
            }
            else
            {
                row.CustomerId = M.CustomerId!.Value;
                row.ProductId = M.ProductId!.Value;
                row.Percent = M.Percent;
                row.IsActive = M.IsActive;
            }
        }

        await _db.SaveChangesAsync();

        if (Request.Headers.ContainsKey("HX-Request"))
        {
            return Content(@"<script>
                window.appToast?.ok('Lưu liên kết thành công.');
                const el = document.getElementById('discModal');
                if (el) bootstrap.Modal.getInstance(el)?.hide();
                window.location.reload();
            </script>", "text/html");
        }

        TempData["Toast.Type"] = "success";
        TempData["Toast.Text"] = "Lưu liên kết thành công.";
        return RedirectToPage("./Index");
    }
}
