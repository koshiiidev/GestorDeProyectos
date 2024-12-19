using GestorDeProyectos.Models;
using GestorDeProyectos.Models.Enums;
using Microsoft.AspNetCore.Identity;

namespace GestorDeProyectos.Data
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            if (!await roleManager.RoleExistsAsync(Models.Enums.UserRoles.Admin))
                await roleManager.CreateAsync(new IdentityRole(Models.Enums.UserRoles.Admin));

            if (!await roleManager.RoleExistsAsync(Models.Enums.UserRoles.User))
                await roleManager.CreateAsync(new IdentityRole(Models.Enums.UserRoles.User));
        }

        public static async Task SeedAdminAsync(UserManager<ApplicationUser> userManager)
        {
            var adminEmail = "admin@gestor.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FirstName = "Administrador",
                    LastName = "Sistema",
                    JobRole = "Administrador"
                };

                var result = await userManager.CreateAsync(admin, "Admin123!");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, Models.Enums.UserRoles.Admin);
                }
            }
        }
    }
}
