using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class LogoutModel : PageModel
{
    private readonly SignInManager<AppUser> _signIn;
    public LogoutModel(SignInManager<AppUser> signIn) => _signIn = signIn;

    /// <summary>
    /// Handle logout and redirect to login page.
    /// </summary>
    /// <returns></returns>
    public async Task<IActionResult> OnPost()
    {
        await _signIn.SignOutAsync();
        return RedirectToPage("/Account/Login");
    }
}
