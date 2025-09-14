using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PosLite.Pages.Status;

public class InternalErrorModel : PageModel
{
    public void OnGet()
    {
        Response.StatusCode = 500;
    }
}
