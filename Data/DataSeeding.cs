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
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
// role seeding
            string[] roles = { "Admin", "LogisticsManager", "Client" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }
// client seeding 
            var clientsToSeed = new List<Client>
    {
        new Client
        {
            Name = "Samsung Electronics",
            ContactDetails = "contact@samsung.com",
            Region = "Gauteng"
        },
        new Client
        {
            Name = "CheckersZA",
            ContactDetails = "userZA@checkers.com | +27 11 549 1234",
            Region = "Kwa-Zulu Natal"
        },
        new Client
        {
            Name = "Batman Distribution",
            ContactDetails = "batman@distribution.com | +27 21 880 5678",
            Region = "Western Cape"
        }
    };

            foreach (var client in clientsToSeed)
            {
                if (!context.Clients.Any(c => c.Name == client.Name))
                {
                    context.Clients.Add(client);
                }
            }

            await context.SaveChangesAsync();

            var samsung = await context.Clients.FirstOrDefaultAsync(c => c.Name == "Samsung Electronics");
            var checkers = await context.Clients.FirstOrDefaultAsync(c => c.Name == "CheckersZA");
            var batman = await context.Clients.FirstOrDefaultAsync(c => c.Name == "Batman Distribution");

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


            var samsungUserEmail = "user@samsung.com";

            if (await userManager.FindByEmailAsync(samsungUserEmail) == null && samsung != null)
            {
                var user = new ApplicationUser
                {
                    UserName = samsungUserEmail,
                    Email = samsungUserEmail,
                    EmailConfirmed = true,
                    ClientID = samsung.ClientID 
                };

                await userManager.CreateAsync(user, "Client123!");
                await userManager.AddToRoleAsync(user, "Client");
            }


            var checkersEmail = "user@checkers.com";

            if (await userManager.FindByEmailAsync(checkersEmail) == null && checkers != null)
            {
                var user = new ApplicationUser
                {
                    UserName = checkersEmail,
                    Email = checkersEmail,
                    EmailConfirmed = true,
                    ClientID = checkers.ClientID 
                };

                await userManager.CreateAsync(user, "Client123!");
                await userManager.AddToRoleAsync(user, "Client");
            }
            var batmanEmail = "user.batman@distribution.com";
            if (await userManager.FindByEmailAsync(batmanEmail) == null && batman != null)
            {
                var user = new ApplicationUser
                {
                    UserName = batmanEmail,
                    Email = batmanEmail,
                    EmailConfirmed = true,
                    ClientID = batman.ClientID
                };

                await userManager.CreateAsync(user, "Client123!");
                await userManager.AddToRoleAsync(user, "Client");
            }

            // manager, not used. was added before i understood the application logic
            var managerEmail = "manager@techmove.com";

            if (await userManager.FindByEmailAsync(managerEmail) == null)
            {
                var manager = new ApplicationUser
                {
                    UserName = managerEmail,
                    Email = managerEmail,
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(manager, "Manager123!");
                await userManager.AddToRoleAsync(manager, "LogisticsManager");
            }
        }
    }
}