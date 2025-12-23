using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sahty.API.Data;
using Sahty.Shared.Auth;

namespace Sahty.Shared.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services, IConfiguration config)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;

        var db = sp.GetRequiredService<ApplicationDbContext>();
        await db.Database.EnsureCreatedAsync();

        var roleMgr = sp.GetRequiredService<RoleManager<IdentityRole>>();
        var userMgr = sp.GetRequiredService<UserManager<ApplicationUser>>();

        // Ensure all roles exist.
        foreach (var r in AppRoles.All)
        {
            if (!await roleMgr.RoleExistsAsync(r))
                await roleMgr.CreateAsync(new IdentityRole(r));
        }

        // Ensure default admin exists.
        var email = config["SeedAdmin:Email"] ?? "admin@sahty.local";
        var pass = config["SeedAdmin:Password"] ?? "Admin123$";
        var name = config["SeedAdmin:FullName"] ?? "Super Admin";

        var admin = await userMgr.FindByEmailAsync(email);
        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FullName = name
            };

            var created = await userMgr.CreateAsync(admin, pass);
            if (!created.Succeeded)
                return;
        }

        if (!await userMgr.IsInRoleAsync(admin, AppRoles.Admin))
            await userMgr.AddToRoleAsync(admin, AppRoles.Admin);
    }

    /// <summary>
    /// Determine the role to assign on first registration.
    /// </summary>
    public static async Task<string> RoleForFirstRegistrationAsync(UserManager<ApplicationUser> userMgr)
    {
        var hasAny = await userMgr.Users.AnyAsync();
        return hasAny ? AppRoles.Patient : AppRoles.Admin;
    }
}
