//using Microsoft.AspNetCore.Identity;
//using Microsoft.EntityFrameworkCore;
//using ST10448420_TechMove_GLMS.Data;
//using ST10448420_TechMove_GLMS.Models;
//using ST10448420_TechMove_GLMS.UtilsServices;
using Microsoft.AspNetCore.Authentication.Cookies;
using ST10448420_TechMove_GLMS.ApiServices;
using ST10448420_TechMove_GLMS.APIServices;

namespace ST10448420_TechMove_GLMS
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            //database connection
            //builder.Services.AddDbContext<ApplicationDbContext>(options =>
            //options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            //setting up identity services for user authentication and authorization
            //builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            //{// the rules for usernames and passwords
            //    options.Password.RequireDigit = true;
            //    options.Password.RequiredLength = 6;
            //    options.Password.RequireUppercase = true;
            //})
            //.AddRoles<IdentityRole>()
            //.AddEntityFrameworkStores<ApplicationDbContext>()
            //.AddDefaultTokenProviders();

            //setting up cookie authentication for managing user sessions and access control,so that each user can have a personalized experience
            //builder.Services.ConfigureApplicationCookie(options =>
            //{
            //    options.LoginPath = "/Account/Login";
            //    options.AccessDeniedPath = "/Account/AccessDenied";
            //});

           
            //DI for the UtilsServices, so that they can be easily used across the application
            //builder.Services.AddScoped<PDFManagementService>();
            //builder.Services.AddHttpClient<CurrencyApiService>();

            // We still use cookies for MVC session management so that [Authorize]
            // attributes and role-based views work. But the identity is now built
            // from the JWT token received from the API.
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                    options.ExpireTimeSpan = TimeSpan.FromHours(8);
                });

            //session handling with JWTs
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(8);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            builder.Services.AddHttpContextAccessor();

            //HttpClient pointing to the backend API
            // In Docker: base address uses the service name "glms-backend-api"
            // In local dev: uses localhost:5001 (adjust to your API launch port)
            var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"]
                             ?? "http://localhost:5001/";

            builder.Services.AddHttpClient("GlmsApi", client =>
            {
                client.BaseAddress = new Uri(apiBaseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            // Register the API service layer (replaces _context calls)
            builder.Services.AddScoped<ApiContractService>();
            builder.Services.AddScoped<ApiServiceRequestService>();
            builder.Services.AddScoped<ApiAuthService>();

            // Add services to the container.establishing MVC 
            builder.Services.AddControllersWithViews();
            var app = builder.Build();

            // Data is now seeded by the API project at startup.
            // using (var scope = app.Services.CreateScope())
            // {
            //     var _services = scope.ServiceProvider;
            //     var _context  = _services.GetRequiredService<ApplicationDbContext>();
            //     _context.Database.EnsureCreated();
            //     await DataSeeding.SeedData(_services);
            // }

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseSession(); // Must come before UseAuthentication
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
