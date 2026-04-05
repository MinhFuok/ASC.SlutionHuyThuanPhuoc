using ASC.Web.Data;
using ASC.WebHuyThuanPhuoc.Configuration;
using ASC.WebHuyThuanPhuoc.Data;
using ASC.WebHuyThuanPhuoc.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// 1. Add services
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddOptions();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 2. Application settings
builder.Services.Configure<ApplicationSettings>(
    builder.Configuration.GetSection("AppSettings"));

// 3. Dependency Injection
builder.Services.AddTransient<IEmailSender, AuthMessageSender>();
builder.Services.AddTransient<ISmsSender, AuthMessageSender>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IIdentitySeed, IdentitySeed>();
builder.Services.AddScoped<DbContext, ApplicationDbContext>();

// 4. Build app
var app = builder.Build();

// 5. Seed dữ liệu
using (var scope = app.Services.CreateScope())
{
    var storageSeed = scope.ServiceProvider.GetRequiredService<IIdentitySeed>();
    await storageSeed.Seed(
        scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>(),
        scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>(),
        scope.ServiceProvider.GetRequiredService<IOptions<ApplicationSettings>>());
}

// 6. Middleware pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
