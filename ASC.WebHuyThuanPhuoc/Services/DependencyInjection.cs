    using ASC.Business;
    using ASC.Business.Interfaces;
    using ASC.DataAccess;
    using ASC.WebHuyThuanPhuoc.Configuration;
    using ASC.WebHuyThuanPhuoc.Data;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;

    namespace ASC.WebHuyThuanPhuoc.Services
    {
        public static class DependencyInjection
        {
            public static IServiceCollection AddConfig(this IServiceCollection services, IConfiguration config)
            {
                var connectionString = config.GetConnectionString("DefaultConnection")
                    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(connectionString));

                services.AddOptions();
                services.Configure<ApplicationSettings>(config.GetSection("AppSettings"));

                services.AddIdentity<IdentityUser, IdentityRole>(options =>
                {
                    options.User.RequireUniqueEmail = true;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

                services.ConfigureApplicationCookie(options =>
                {
                    options.LoginPath = "/Identity/Account/Login";
                    options.LogoutPath = "/Identity/Account/Logout";
                    options.AccessDeniedPath = "/Identity/Account/Login";
                });

                var googleIdentitySection = config.GetSection("Authentication:Google:Identity");
                var googleClientId = googleIdentitySection["ClientId"];
                var googleClientSecret = googleIdentitySection["ClientSecret"];

                if (!string.IsNullOrWhiteSpace(googleClientId) &&
                    !string.IsNullOrWhiteSpace(googleClientSecret))
                {
                    services
                        .AddAuthentication()
                        .AddGoogle(options =>
                        {
                            options.ClientId = googleClientId;
                            options.ClientSecret = googleClientSecret;
                            options.CallbackPath = "/signin-google";
                        });
                }

                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = config.GetSection("CacheSettings:CacheConnectionString").Value;
                    options.InstanceName = config.GetSection("CacheSettings:CacheInstance").Value;
                });

                return services;
            }

            public static IServiceCollection AddMyDependencyGroup(this IServiceCollection services)
            {
                services.AddScoped<IMasterDataOperations, MasterDataOperations>();
                services.AddScoped<IMasterDataCacheOperations, MasterDataCacheOperations>();
                services.AddScoped<IServiceRequestOperations, ServiceRequestOperations>();

                services.AddControllersWithViews();
                services.AddRazorPages();

                services.AddTransient<IEmailSender, AuthMessageSender>();
                services.AddTransient<ISmsSender, AuthMessageSender>();

                services.AddSingleton<IIdentitySeed, IdentitySeed>();
                services.AddScoped<DbContext, ApplicationDbContext>();
                services.AddScoped<UnitOfWork, UnitOfWork>();
                services.AddScoped<IUnitOfWork, UnitOfWork>();

                services.AddMemoryCache();
                services.AddScoped<INavigationCacheOperations, NavigationCacheOperations>();

                services.AddAutoMapper(typeof(ApplicationDbContext));
  
                services.AddSession();
                services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

                return services;
            }
    }
    }
