using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PosLite.Pages.Users;

public class EditModalModel : PageModel
{
    private readonly UserManager<AppUser> _um;
    public EditModalModel(UserManager<AppUser> um) => _um = um;

    [BindProperty(SupportsGet = true)] public string Id { get; set; } = "";

    [BindProperty] public InputModel Input { get; set; } = new();

    public class InputModel
    {
        public string Email { get; set; } = "";
        public string UserName { get; set; } = "";
        [Phone] public string? PhoneNumber { get; set; }
    }

    public async Task<IActionResult> OnGet()
    {
        var u = await _um.FindByIdAsync(Id);
        if (u == null) return NotFound();

        if (u.Email != null && u.Email.Equals("admin@local.com", StringComparison.OrdinalIgnoreCase))
            return Forbid();

        Input = new InputModel { Email = u.Email!, UserName = u.UserName ?? "", PhoneNumber = u.PhoneNumber };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var u = await _um.FindByIdAsync(Id);
        if (u == null) return NotFound();

        if (u.Email != null && u.Email.Equals("admin@local.com", StringComparison.OrdinalIgnoreCase))
            return Forbid();

        var byEmail = await _um.FindByEmailAsync(Input.Email);

        if (byEmail != null && byEmail.Id != u.Id)
        {
            ModelState.AddModelError(nameof(Input.Email), "Email đã tồn tại."); return Page();
        }
        var byName = await _um.FindByNameAsync(Input.UserName);

        if (byName != null && byName.Id != u.Id)
        {
            ModelState.AddModelError(nameof(Input.UserName), "Tên đăng nhập đã tồn tại."); return Page();
        }

        //await _um.SetEmailAsync(u, Input.Email);
        //await _um.SetUserNameAsync(u, Input.UserName);
        u.PhoneNumber = Input.PhoneNumber;
        var res = await _um.UpdateAsync(u);

        TempData["Toast.Type"] = "success";
        TempData["Toast.Text"] = "Cập nhật người dùng thành công.";

        if (!res.Succeeded)
        {
            foreach (var e in res.Errors) ModelState.AddModelError(string.Empty, e.Description);
            return Page();
        }

        return Content(@"<script>
            window.appToast?.ok('Đã cập nhật người dùng.');
            const el = document.getElementById('userModal');
            if (el) bootstrap.Modal.getInstance(el)?.hide();
            window.location.reload();
        </script>", "text/html");
    }
}
