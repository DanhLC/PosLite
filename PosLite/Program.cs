using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// DbContext + SQLite
builder.Services.AddDbContext<AppDb>(o =>
    o.UseSqlite(builder.Configuration.GetConnectionString("AppDb")));

// Identity
builder.Services.AddIdentity<AppUser, IdentityRole>(opt =>
{
    opt.SignIn.RequireConfirmedAccount = false;
    opt.Password.RequireDigit = false;
    opt.Password.RequireLowercase = false;
    opt.Password.RequireNonAlphanumeric = false;
    opt.Password.RequireUppercase = false;
    opt.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<AppDb>()
.AddDefaultTokenProviders();

// Cookie (đi đúng trang login custom)
builder.Services.ConfigureApplicationCookie(o =>
{
    o.LoginPath = "/Account/Login";
    o.AccessDeniedPath = "/Account/Login";
    o.SlidingExpiration = true;
    o.ExpireTimeSpan = TimeSpan.FromDays(30); 
});

// Razor Pages + mở ẩn danh cho trang login
builder.Services.AddRazorPages().AddRazorPagesOptions(opt =>
{
    opt.Conventions.AllowAnonymousToPage("/Account/Login");
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Yêu cầu đăng nhập cho toàn bộ site trừ các trang AllowAnonymous ở trên
app.MapRazorPages().RequireAuthorization();

// Tạo DB + bật WAL + seed Admin
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDb>();
    db.Database.Migrate();
    db.Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;");
    await Seed.CreateRolesAndAdmin(scope.ServiceProvider);
}


app.Run();
