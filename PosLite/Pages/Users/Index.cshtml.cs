using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace PosLite.Pages.Users;

[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly UserManager<AppUser> _userManager;
    public IndexModel(UserManager<AppUser> userManager) => _userManager = userManager;

    [BindProperty(SupportsGet = true)] public string? q { get; set; }
    [BindProperty(SupportsGet = true)] public int pageIndex { get; set; } = 1;
    [BindProperty(SupportsGet = true)] public int pageSize { get; set; } = 10;

    public int TotalItems { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / Math.Max(1, pageSize));

    public List<Row> Items { get; set; } = new();

    public class Row
    {
        public string Id { get; set; } = default!;
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public string? Phone { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool LockoutEnabled { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public bool IsBase { get; set; }
    }

    public async Task OnGet()
    {
        var users = _userManager.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            users = users.Where(u =>
                (u.Email != null && u.Email.ToLower().Contains(term)) ||
                (u.UserName != null && u.UserName.ToLower().Contains(term)) ||
                (u.PhoneNumber != null && u.PhoneNumber.Contains(term)));
        }

        TotalItems = await users.CountAsync();

        if (pageIndex < 1) pageIndex = 1;
        if (pageSize < 1) pageSize = 10;
        var skip = (pageIndex - 1) * pageSize;

        Items = await users
            .OrderBy(u => u.Email)
            .Skip(skip).Take(pageSize)
            .Select(u => new Row
            {
                Id = u.Id,
                Email = u.Email,
                UserName = u.UserName,
                Phone = u.PhoneNumber,
                EmailConfirmed = u.EmailConfirmed,
                LockoutEnabled = u.LockoutEnabled,
                LockoutEnd = u.LockoutEnd,
                IsBase = u.Email != null &&
                     u.Email.Equals("admin@local.com", StringComparison.OrdinalIgnoreCase)
            }).ToListAsync();
    }
}
