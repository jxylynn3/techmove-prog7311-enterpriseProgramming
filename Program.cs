using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ST10448420_TechMove_GLMS.ApiServices;
using ST10448420_TechMove_GLMS.Data;
using ST10448420_TechMove_GLMS.Models;
//using ST10448420_TechMove_GLMS.UtilsServices; // commented out — moved to API project in Part 3

namespace ST10448420_TechMove_GLMS
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ── DATABASE — kept for UserManager/RoleManager in AdminController ──────────────
            // The MVC no longer uses DbContext for contracts or service requests.
            // Only kept so UserManager<ApplicationUser> and RoleManager<IdentityRole> work
            // for the Admin panel's user management pages.
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // ── IDENTITY ──────────────────────────────────────────────────────────────────────
            // AddIdentity registers four schemes internally:
            //   "Identity.Application" — this is the real sign-in cookie scheme
            //   "Identity.External", "Identity.TwoFactorRememberMe", "Identity.TwoFactorUserId"
            // It also sets DefaultAuthenticateScheme = DefaultChallengeScheme = "Identity.Application".
            // We do NOT fight this. We use "Identity.Application" everywhere.
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireUppercase = true;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // ── THE FIX: Configure the Identity.Application cookie correctly ─────────────────
            // We do NOT add a new "Cookies" scheme — AddIdentity already owns that.
            // ConfigureApplicationCookie configures the "Identity.Application" scheme that
            // AddIdentity registered. [Authorize] will challenge against this scheme by default.
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
            });

            // ── COMMENTED OUT FROM PART 2 ────────────────────────────────────────────────────
            // builder.Services.ConfigureApplicationCookie(options =>
            // {
            //     options.LoginPath = "/Account/Login";
            //     options.AccessDeniedPath = "/Account/AccessDenied";
            // });
            // builder.Services.AddScoped<PDFManagementService>();    // moved to API project
            // builder.Services.AddHttpClient<CurrencyApiService>();  // moved to API project
            // ────────────────────────────────────────────────────────────────────────────────

            // ── SESSION — stores the JWT token so API service classes can attach it ─────────
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(8);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            builder.Services.AddHttpContextAccessor();

            // ── HTTPCLIENT — points to the backend API ────────────────────────────────────────
            var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"]
                             ?? "http://localhost:5245/";

            builder.Services.AddHttpClient("GlmsApi", client =>
            {
                client.BaseAddress = new Uri(apiBaseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            // ── API SERVICE LAYER — replaces all _context.Contracts/_context.ServiceRequests ──
            builder.Services.AddScoped<ApiContractService>();
            builder.Services.AddScoped<ApiServiceRequestService>();
            builder.Services.AddScoped<ApiAuthService>();

            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // ── COMMENTED OUT FROM PART 2 — seeding now done by the API project ─────────────
            // using (var scope = app.Services.CreateScope())
            // {
            //     var _services = scope.ServiceProvider;
            //     var _context  = _services.GetRequiredService<ApplicationDbContext>();
            //     _context.Database.EnsureCreated();
            //     await DataSeeding.SeedData(_services);
            // }
            // ────────────────────────────────────────────────────────────────────────────────

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseSession();           // MUST come before UseAuthentication
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
            //heyy bestie, pls work
        }
    }
}