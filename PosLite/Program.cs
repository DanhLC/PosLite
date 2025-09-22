using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using PosLite.Common;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// ================= DB Context (SQLite) =================
var dataDir = Path.Combine(Directory.GetCurrentDirectory(), "data");
if (!Directory.Exists(dataDir))
    Directory.CreateDirectory(dataDir);

var dbPath = Path.Combine(dataDir, "pos.db");
builder.Services.AddDbContext<AppDb>(o =>
    o.UseSqlite($"Data Source={dbPath}"));

// ================= Identity =================
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

// ================= Cookie =================
builder.Services.ConfigureApplicationCookie(o =>
{
    o.LoginPath = "/dang-nhap";
    o.AccessDeniedPath = "/dang-nhap";
    o.SlidingExpiration = true;
    o.ExpireTimeSpan = TimeSpan.FromDays(30);
});

// ================= Razor Pages =================
builder.Services.AddRazorPages().AddRazorPagesOptions(opt =>
{
    opt.Conventions.AllowAnonymousToPage("/Account/Login");
    opt.Conventions.AllowAnonymousToPage("/Status/404");
    opt.Conventions.AllowAnonymousToPage("/Status/500");
    opt.Conventions.AllowAnonymousToPage("/Status/403");
});

// ================= Dependency Injection =================
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

var app = builder.Build();

// ================= Middleware =================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Status/500");
    app.UseStatusCodePagesWithReExecute("/Status/404");
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// ================= Chặn IP ngoài LAN =================
if (app.Environment.IsProduction())
{
    app.Use(async (context, next) =>
    {
        var path = context.Request.Path.Value;

        if (path != null && path.StartsWith("/403"))
        {
            await next();
            return;
        }

        var remoteIp = context.Connection.RemoteIpAddress;
        if (remoteIp != null && !remoteIp.ToString().StartsWith("192.168."))
        {
            context.Response.Redirect("/403");
            return;
        }

        await next();
    });
}


// ================= Razor Pages =================
app.MapRazorPages().RequireAuthorization();

// ================= DB Migration + Seed =================
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

// ================= Localization =================
var vi = new CultureInfo("vi-VN");
var locOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(vi),
    SupportedCultures = new[] { vi },
    SupportedUICultures = new[] { vi }
};
app.UseRequestLocalization(locOptions);

app.Run();
