using ASC.Web.Data;
using ASC.WebHuyThuanPhuoc.Configuration;
using ASC.WebHuyThuanPhuoc.Data;
using ASC.WebHuyThuanPhuoc.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Options;
using ASC.Web.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình các dịch vụ hệ thống (Add services to the container)
builder.Services.AddControllersWithViews();

// 2. Cấu hình Options và ApplicationSettings
builder.Services.AddOptions();
// Lưu ý: Section trong appsettings.json của bạn là "ApplicationSettings" hay "AppSettings"? 
// Theo Lab thì thường là "ApplicationSettings".
builder.Services.Configure<ApplicationSettings>(builder.Configuration.GetSection("AppSettings"));

// 3. Đăng ký các dịch vụ ứng dụng (Dependency Injection)
// Phải đăng ký TRƯỚC khi Build
builder.Services.AddTransient<IEmailSender, AuthMessageSender>();
builder.Services.AddTransient<ISmsSender, AuthMessageSender>();

// Đăng ký DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Đăng ký Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IIdentitySeed, IdentitySeed>();
builder.Services.AddScoped<DbContext, ApplicationDbContext>();

// 4. Xây dựng ứng dụng
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var storageSeed = scope.ServiceProvider.GetRequiredService<IIdentitySeed>();
    await storageSeed.Seed(
        scope.ServiceProvider.GetService<UserManager<IdentityUser>>(),
        scope.ServiceProvider.GetService<RoleManager<IdentityRole>>(),
        scope.ServiceProvider.GetService<IOptions<ApplicationSettings>>());
}

// 5. Cấu hình HTTP request pipeline (Middleware)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();