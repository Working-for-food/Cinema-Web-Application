using Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Data.Seed;

public static class RoleInitializer
{
    public static async Task InitializeAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        string[] roleNames = { "admin", "user" };

        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        string adminEmail = "superadmin@gmail.com";
        string adminPassword = "SuperAdmin123@";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            var newAdmin = new ApplicationUser
            {
                UserName = "superadmin",
                Email = adminEmail,
                EmailConfirmed = true,
                DateOfBirth = new DateOnly(1990, 1, 1)
            };

            var createResult = await userManager.CreateAsync(newAdmin, adminPassword);

            if (createResult.Succeeded)
            {
                await userManager.AddToRoleAsync(newAdmin, "admin");
            }
        }
    }
}