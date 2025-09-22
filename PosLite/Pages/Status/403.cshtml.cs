using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PosLite.Pages.Status;

public class ForbiddenModel : PageModel
{
    public void OnGet()
    {
        Response.StatusCode = 403;
    }
}
