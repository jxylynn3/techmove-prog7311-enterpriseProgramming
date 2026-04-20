using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ST10448420_TechMove_GLMS.Data;
using ST10448420_TechMove_GLMS.Models;
using ST10448420_TechMove_GLMS.UtilsServices;

namespace ST10448420_TechMove_GLMS
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            //database connection
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            //setting up identity services for user authentication and authorization
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {// the rules for usernames and passwords
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireUppercase = true;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            //setting up cookie authentication for managing user sessions and access control,so that each user can have a personalized experience
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
            });

            // Add services to the container.establishing MVC 
            builder.Services.AddControllersWithViews();

            var app = builder.Build();
            // Seed initial data (roles and admin user)
            using (var scope = app.Services.CreateScope())
            {
                var _services = scope.ServiceProvider;
                var _context = _services.GetRequiredService<ApplicationDbContext>();

                ////run then delete
                try
                {
                    // This forces the DB to be created based on your migrations folder.
                    // Once the DB shows up in SQL Object Explorer, you can comment these two lines out again.
                    _context.Database.EnsureCreated();

                    // Seed initial data
                    await DataSeeding.SeedData(_services);
                }
                catch (Exception ex)
                {
                    // Exception handlingg for seeding data.
                    var _logger = _services.GetRequiredService<ILogger<Program>>();
                    _logger.LogError(ex, "An error occurred while seeding to ST10448420_TechMove_GLMS database.");
                }
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            //authentication and authorization middleware
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
            //heyy bestie,pls work
        }
    }
}
