using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ST10448420_TechMove_GLMS.Models;

namespace ST10448420_TechMove_GLMS.Data
{
    public class DataSeeding
    {
        public static async Task SeedData(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            // Access the DbContext to seed the Client table
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // 1. Seed the Roles
            string[] roles = { "Admin", "LogisticsManager", "Client" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // 2. Seed a Default Client Company (TechMove Partner)
            // We check if any client exists; if not, we create one.
            if (!context.Clients.Any())
            {
                context.Clients.Add(new Client
                {
                    Name = "Samsung Electronics",
                    ContactDetails = "contact@samsung.com",
                    Region = "Gauteng"
                });
                await context.SaveChangesAsync();
            }

            // Get the ID of the client we just ensured exists
            var defaultClient = await context.Clients.FirstOrDefaultAsync(c => c.Name == "Samsung Electronics");

            // 3. Seed Default Admin User
            var adminEmail = "admin@techmove.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(admin, "Admin123!");
                await userManager.AddToRoleAsync(admin, "Admin");
            }

            // 4. Seed a Default Client User (Linked to Samsung)
            var clientEmail = "user@samsung.com";
            if (await userManager.FindByEmailAsync(clientEmail) == null && defaultClient != null)
            {
                var clientUser = new ApplicationUser
                {
                    UserName = clientEmail,
                    Email = clientEmail,
                    EmailConfirmed = true,
                    // Linking the user to the company
                    ClientID = defaultClient.ClientID
                };

                var result = await userManager.CreateAsync(clientUser, "Client123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(clientUser, "Client");
                }
            }
        }
    }
}