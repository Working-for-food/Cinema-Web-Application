using Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration; 

namespace Infrastructure.Data.Seed;

public static class RoleInitializer
{
    public static async Task InitializeAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration)
    {
        string[] roleNames = { "admin", "user" };

        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        string adminEmail = configuration["SuperAdmin:Email"];
        string adminPassword = configuration["SuperAdmin:Password"];

        if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
        {
            return;
        }

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