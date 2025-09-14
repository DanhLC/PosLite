using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PosLite.Pages.Status;

public class NotFoundModel : PageModel
{
    public void OnGet()
    {
        Response.StatusCode = 404;
    }
}
