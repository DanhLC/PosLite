using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace PosLite.Pages.Customers;

public class FormModalModel : PageModel
{
    private readonly AppDb _db;
    public FormModalModel(AppDb db) => _db = db;

    [BindProperty(SupportsGet = true)] public Guid? Id { get; set; }
    [BindProperty] public InputModel M { get; set; } = new();

    public class InputModel
    {
        [Required, StringLength(50)]
        public string Code { get; set; } = "";
        [Required, StringLength(200)]
        public string Name { get; set; } = "";
        [Phone, StringLength(30)]
        public string? Phone { get; set; }
        [StringLength(300)]
        public string? Address { get; set; }
        public bool IsActive { get; set; } = true;
    }

    private static string GenCode() => "KH" + Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();

    public async Task OnGet()
    {
        if (Id.HasValue)
        {
            var c = await _db.Customers.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.CustomerId == Id.Value);

            if (c != null)
            {
                M = new InputModel
                {
                    Code = c.Code,
                    Name = c.Name,
                    Phone = c.Phone,
                    Address = c.Address,
                    IsActive = c.IsActive
                };
            }
        }
        else
        {
            M.Code = GenCode();
            M.IsActive = true;
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await OnGet();
            return Page();
        }

        var codeExists = await _db.Customers.IgnoreQueryFilters()
            .AnyAsync(x => x.Code == M.Code.Trim() && x.CustomerId != Id);
        if (codeExists)
        {
            ModelState.AddModelError("M.Code", "Mã khách hàng đã tồn tại.");
            await OnGet();
            return Page();
        }

        if (Id == null)
        {
            var c = new Customer
            {
                CustomerId = Guid.NewGuid(),
                Code = M.Code.Trim(),
                Name = M.Name.Trim(),
                Phone = string.IsNullOrWhiteSpace(M.Phone) ? null : M.Phone!.Trim(),
                Address = string.IsNullOrWhiteSpace(M.Address) ? null : M.Address!.Trim(),
                IsActive = M.IsActive
            };
            _db.Customers.Add(c);

            TempData["Toast.Type"] = "success";
            TempData["Toast.Text"] = "Lưu khách hàng thành công.";
        }
        else
        {
            var c = await _db.Customers.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.CustomerId == Id.Value);
            if (c == null) return NotFound();

            c.Code = M.Code.Trim();
            c.Name = M.Name.Trim();
            c.Phone = string.IsNullOrWhiteSpace(M.Phone) ? null : M.Phone!.Trim();
            c.Address = string.IsNullOrWhiteSpace(M.Address) ? null : M.Address!.Trim();
            c.IsActive = M.IsActive;

            TempData["Toast.Type"] = "success";
            TempData["Toast.Text"] = "Đã cập nhật khách hàng thành công.";
        }

        await _db.SaveChangesAsync();

        if (Request.Headers.ContainsKey("HX-Request"))
        {
            var html = @"<script>
                window.appToast?.ok('Lưu khách hàng thành công.');
                const el = document.getElementById('custModal');
                if (el) bootstrap.Modal.getInstance(el)?.hide();
                window.location.reload();
            </script>";
            return Content(html, "text/html");
        }

        return RedirectToPage("./Index");
    }
}
