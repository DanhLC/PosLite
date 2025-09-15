using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PosLite.Common;

namespace PosLite.Pages.Categories;

public class FormModalModel : PageModel
{
    private readonly AppDb _db;
    public FormModalModel(AppDb db) => _db = db;

    // query id (null => create)
    [BindProperty(SupportsGet = true)] public Guid? Id { get; set; }
    public bool IsEdit => Id.HasValue;

    // form fields
    [BindProperty, Required, StringLength(200)]
    public string Name { get; set; } = "";
    [BindProperty] public bool IsActive { get; set; } = true;

    /// <summary>
    /// Load data for edit
    /// </summary>
    /// <returns></returns>
    public async Task<IActionResult> OnGetAsync()
    {
        if (IsEdit)
        {
            var cat = await _db.Categories.IgnoreQueryFilters()
                          .FirstOrDefaultAsync(x => x.CategoryId == Id);

            if (cat == null) return NotFound();

            Name = cat.Name;
            IsActive = cat.IsActive;
        }

        return Page();
    }

    /// <summary>
    /// Save (create or update)
    /// </summary>
    /// <returns></returns>
    public async Task<IActionResult> OnPostAsync()
    {
        Name = (Name ?? string.Empty).Trim();

        if (!ModelState.IsValid) return Page();

        try
        {
            var norm = TextSearch.Normalize(Name);
            var id = Id ?? Guid.Empty;
            var exists = await _db.Categories
                .IgnoreQueryFilters()
                .AnyAsync(x => x.CategoryId != id && x.NameSearch == norm);

            if (exists)
            {
                ModelState.AddModelError(nameof(Name), "Tên danh mục đã tồn tại.");
                return Page();
            }

            if (IsEdit)
            {
                var cat = await _db.Categories.IgnoreQueryFilters()
                    .FirstOrDefaultAsync(x => x.CategoryId == Id);
                if (cat == null) return NotFound();
                cat.Name = Name.Trim();
                cat.IsActive = IsActive;
                TempData["Toast.Type"] = "success";
                TempData["Toast.Text"] = "Đã cập nhật danh mục.";
            }
            else
            {
                _db.Categories.Add(new Category
                {
                    CategoryId = Guid.NewGuid(),
                    Name = Name.Trim(),
                    IsActive = true
                });
                TempData["Toast.Type"] = "success";
                TempData["Toast.Text"] = "Đã tạo danh mục.";
            }

            await _db.SaveChangesAsync();

            var js = """
                 <script>
                   const m = bootstrap.Modal.getInstance(document.getElementById('catModal'));
                   if (m) m.hide();
                   window.location.reload();
                 </script>
                 """;
            return Content(js, "text/html");
        }
        catch (Exception)
        {
            var js = """
                 <script>
                   if (window.appToast) appToast.err('Lưu thất bại. Vui lòng thử lại!');
                 </script>
                 """;
            return Content(js, "text/html");
        }
    }
}
