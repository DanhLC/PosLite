using System.Security.Claims;

public interface ICurrentUser
{
    string? UserId { get; }
    string? UserName { get; }
}

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _ctx;
    public CurrentUser(IHttpContextAccessor ctx) => _ctx = ctx;

    public string? UserId => _ctx.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
    public string? UserName => _ctx.HttpContext?.User.Identity?.Name;
}
