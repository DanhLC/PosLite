using Microsoft.AspNetCore.Identity;

public static class Seed
{
    public static async Task CreateRolesAndAdmin(IServiceProvider sp)
    {
        var roleMgr = sp.GetRequiredService<RoleManager<IdentityRole>>();
        var userMgr = sp.GetRequiredService<UserManager<AppUser>>();

        foreach (var r in new[] { "Admin", "Cashier" })
            if (!await roleMgr.RoleExistsAsync(r))
                await roleMgr.CreateAsync(new IdentityRole(r));

        var admin = await userMgr.FindByEmailAsync("admin@local.com");

        if (admin == null)
        {
            admin = new AppUser
            {
                UserName = "admin@local.com",
                Email = "admin@local.com",
                EmailConfirmed = true
            };
            await userMgr.CreateAsync(admin, "admin@local.com");
            await userMgr.AddToRoleAsync(admin, "Admin");
        }
    }
}
