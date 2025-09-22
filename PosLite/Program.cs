using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using PosLite.Common;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// DbContext + SQLite
var dataDir = Path.Combine(Directory.GetCurrentDirectory(), "data");
if (!Directory.Exists(dataDir))
    Directory.CreateDirectory(dataDir);

var dbPath = Path.Combine(dataDir, "pos.db");
builder.Services.AddDbContext<AppDb>(o =>
    o.UseSqlite($"Data Source={dbPath}"));

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
    opt.Conventions.AllowAnonymousToPage("/Status/404");
    opt.Conventions.AllowAnonymousToPage("/Status/500");
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(5000);
});

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseExceptionHandler("/500");
app.UseStatusCodePagesWithReExecute("/404");
app.UseAuthorization();
app.MapRazorPages().RequireAuthorization();

// Tạo DB + bật WAL + seed Admin
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDb>();
    db.Database.Migrate();
    db.Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;");
    await Seed.CreateRolesAndAdmin(scope.ServiceProvider);

    var needFix = await db.Categories
       .IgnoreQueryFilters()
       .Where(c => string.IsNullOrEmpty(c.NameSearch))
       .ToListAsync();

    foreach (var c in needFix)
        c.NameSearch = TextSearch.Normalize(c.Name);

    if (needFix.Count > 0)
        await db.SaveChangesAsync();
}

var vi = new CultureInfo("vi-VN");
var locOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(vi),
    SupportedCultures = new[] { vi },
    SupportedUICultures = new[] { vi }
};

app.UseRequestLocalization(locOptions);
app.Run();
