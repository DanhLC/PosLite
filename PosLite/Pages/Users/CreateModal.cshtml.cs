using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PosLite.Pages.Users;

/// <summary>
/// Page model for creating a new user via a modal dialog.
/// </summary>
public class CreateModalModel : PageModel
{
    private readonly UserManager<AppUser> _um;
    public CreateModalModel(UserManager<AppUser> um) => _um = um;

    [BindProperty] public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required, EmailAddress] public string Email { get; set; } = "";
        [Required] public string UserName { get; set; } = "";
        [Phone] public string? PhoneNumber { get; set; }
        [Required, StringLength(100, MinimumLength = 6)] public string Password { get; set; } = "";
        [Compare(nameof(Password))] public string ConfirmPassword { get; set; } = "";
    }

    /// <summary>
    /// Handle GET requests.
    /// </summary>
    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var exEmail = await _um.FindByEmailAsync(Input.Email);

        if (exEmail != null)
        {
            ModelState.AddModelError(nameof(Input.Email), "Email đã tồn tại.");
            return Page();
        }

        var exName = await _um.FindByNameAsync(Input.UserName);

        if (exName != null)
        {
            ModelState.AddModelError(nameof(Input.UserName), "Tên đăng nhập đã tồn tại.");
            return Page();
        }

        var u = new AppUser
        {
            Email = Input.Email,
            UserName = Input.UserName,
            PhoneNumber = Input.PhoneNumber,
            EmailConfirmed = true
        };

        var res = await _um.CreateAsync(u, Input.Password);

        TempData["Toast.Type"] = "success";
        TempData["Toast.Text"] = "Tạo người dùng thành công.";

        if (!res.Succeeded)
        {
            foreach (var e in res.Errors) ModelState.AddModelError(string.Empty, e.Description);
            return Page();
        }

        return Content(@"<script>
            window.appToast?.ok('Đã tạo người dùng.');
            const el = document.getElementById('userModal');
            if (el) bootstrap.Modal.getInstance(el)?.hide();
            window.location.reload();
        </script>", "text/html");
    }
}
