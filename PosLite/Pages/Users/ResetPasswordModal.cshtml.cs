using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace PosLite.Pages.Users;

[Authorize(Roles = "Admin")]
public class ResetPasswordModalModel : PageModel
{
    private readonly UserManager<AppUser> _userManager;
    public ResetPasswordModalModel(UserManager<AppUser> userManager) => _userManager = userManager;

    [BindProperty(SupportsGet = true)] public string Id { get; set; } = default!;
    [BindProperty] public InputModel M { get; set; } = new();

    public string? Email { get; set; }

    public class InputModel
    {
        [Required, StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu 6–100 ký tự")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = "";

        [Required, DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "Mật khẩu nhập lại không khớp")]
        public string ConfirmPassword { get; set; } = "";
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var u = await _userManager.FindByIdAsync(Id);
        if (u == null) return NotFound();
        Email = u.Email;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var u = await _userManager.FindByIdAsync(Id);
        if (u == null) return NotFound();

        if (!ModelState.IsValid)
        {
            Email = u.Email;
            return Page();
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(u);
        var result = await _userManager.ResetPasswordAsync(u, token, M.NewPassword);

        TempData["Toast.Type"] = "success";
        TempData["Toast.Text"] = "Đã đặt lại mật khẩu.";

        if (!result.Succeeded)
        {
            foreach (var e in result.Errors) ModelState.AddModelError(string.Empty, e.Description);
            Email = u.Email;
            return Page();
        }

        return Content(@"<script>
            window.appToast?.ok('Đã đặt lại mật khẩu.');
            const el = document.getElementById('resetPwdModal');
            if (el) bootstrap.Modal.getOrCreateInstance(el).hide();
            setTimeout(()=>window.location.reload(), 120);
        </script>", "text/html");
    }
}
