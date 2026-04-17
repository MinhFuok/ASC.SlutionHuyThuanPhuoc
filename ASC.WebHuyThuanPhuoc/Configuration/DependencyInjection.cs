using ASC.Web.Data;
using ASC.WebHuyThuanPhuoc.Data;
using ASC.WebHuyThuanPhuoc.Services;
using ASC.WebHuyThuanPhuoc.Configuration;
using ASC.WebHuyThuanPhuoc.Operations;
using ASC.Business;
using ASC.Business.Interfaces;
using ASC.DataAccess;
using ASC.WebHuyThuanPhuoc.Areas.Configuration.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ASC.WebHuyThuanPhuoc.Configuration
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddMyDependencyGroup(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddOptions();

            services.Configure<ApplicationSettings>(
                configuration.GetSection("AppSettings"));

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.User.RequireUniqueEmail = true;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 6;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Identity/Account/Login";
                options.LogoutPath = "/Identity/Account/Logout";
                options.AccessDeniedPath = "/Identity/Account/Login";
            });

            var googleIdentitySection = configuration.GetSection("Authentication:Google:Identity");
            var googleClientId = googleIdentitySection["ClientId"];
            var googleClientSecret = googleIdentitySection["ClientSecret"];

            if (!string.IsNullOrWhiteSpace(googleClientId) &&
                !string.IsNullOrWhiteSpace(googleClientSecret))
            {
                services.AddAuthentication()
                    .AddGoogle(options =>
                    {
                        options.ClientId = googleClientId;
                        options.ClientSecret = googleClientSecret;
                        options.CallbackPath = "/signin-google";
                    });
            }

            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();

            services.AddSingleton<IIdentitySeed, IdentitySeed>();
            services.AddScoped<DbContext, ApplicationDbContext>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IMasterDataOperations, MasterDataOperations>();
            services.AddAutoMapper(typeof(MappingProfile));

            services.AddMemoryCache();
            services.AddScoped<INavigationCacheOperations, NavigationCacheOperations>();

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            return services;
        }
    }
}
