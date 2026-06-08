using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ST10448420_TechMove_GLMS.ApiServices;
using ST10448420_TechMove_GLMS.Data;
using ST10448420_TechMove_GLMS.Models;
using ST10448420_TechMove_GLMS.UtilsServices; // Re-enabled in Part 3 — ContractController still needs PDFManagementService
                                              // for local file uploads. PDF creation was NOT moved to the API because
                                              // the API receives JSON, not multipart form data.

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
            // for the Admin panel's user management pages, and so ContractController can
            // load the Clients list for its Create/Edit dropdowns.
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
            // builder.Services.AddHttpClient<CurrencyApiService>();  // moved to API project
            // ────────────────────────────────────────────────────────────────────────────────

            // ── PDF SERVICE — Re-registered here for ContractController ───────────────────────
            // ContractController still handles PDF file uploads directly (multipart form data).
            // The API receives JSON only, so PDF saving stays in the MVC layer.
            // builder.Services.AddScoped<PDFManagementService>();    // moved to API project (Part 02 comment kept for CI evidence)
            builder.Services.AddScoped<PDFManagementService>(); // Re-enabled: ContractController requires this for file upload

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

            // MVC databse initialization and seeding.
            // EnsureCreated() creates the MVC's LocalDB schema if it does not exist yet.
            // This fixes "Cannot open database ST10448420_TechMove_GLMS_DB" on new machines.
            // DataSeeding populates roles, clients, and users into the MVC database.
            // All seed operations use guard checks (e.g. FindByEmailAsync == null) so they
            // are safe to run on every startup without creating duplicate records.
            //
            // seeding is done by API now
            // using (var scope = app.Services.CreateScope())
            // {
            //     var _services = scope.ServiceProvider;
            //     var _context  = _services.GetRequiredService<ApplicationDbContext>();
            //     _context.Database.EnsureCreated();
            //     await DataSeeding.SeedData(_services);
            // }
           
            // Re-enabled in Part 3: EnsureCreated + DataSeeding run for the MVC database only.
            // The API has its own separate database (ST10448420_TechMove_GLMS_API_DB) and its
            // own DataSeeding — these two seedings are completely independent.
            using (var scope = app.Services.CreateScope())
            {
                var _services = scope.ServiceProvider;
                var _context = _services.GetRequiredService<ApplicationDbContext>();
                try
                {
                    _context.Database.EnsureCreated();
                    await DataSeeding.SeedData(_services);
                }
                catch (Exception ex)
                {
                    var logger = _services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while bootstrapping the MVC database.");
                }
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            // Docker fix: Skip HTTPS redirection when running inside a container.
            // Kestrel runs on plain HTTP (port 8080) in Docker — there is no TLS certificate
            // available inside the image. HTTPS termination would be handled by a load balancer
            // or reverse proxy in a real production environment.
            // The ASPNETCORE_ENVIRONMENT is set to "Docker" in docker-compose.yml,
            // so we check for that specific value here.
            if (!string.Equals(app.Environment.EnvironmentName, "Docker",
                    StringComparison.OrdinalIgnoreCase))
            {

                app.UseHttpsRedirection();
            }
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