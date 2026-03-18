using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Configuration;
using System.Globalization;
using System.Net.Http.Headers;
using VBSPOSS.Areas.Identity.Data;
using VBSPOSS.Data;
using VBSPOSS.Filters;
using VBSPOSS.Helpers.Interfaces;
using VBSPOSS.Implements.Helpers;
using VBSPOSS.Integration.Implements;
using VBSPOSS.Integration.Interfaces;
using VBSPOSS.Mappings;
using VBSPOSS.Services.Implements;
using VBSPOSS.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Lấy chuỗi kết nối
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddDbContext<MyIdentityDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<MyIdentityDbContext>();

// Add services to the container.
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation(); // Bỏ AuthorizeFilter global để [AllowAnonymous] hoạt động

builder.Services.AddMvc().AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);
builder.Services.AddKendo();
builder.Services.AddAutoMapper(typeof(MappingProfile));

//add
builder.Services.AddScoped<IProductService, ProductService>();


// Đăng ký IHttpContextAccessor
builder.Services.AddHttpContextAccessor();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.Name = ".VBSPOSS.Session"; // Đặt tên tùy chỉnh cho cookie
});

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddSingleton<ISessionHelper, SessionHelper>();
builder.Services.AddScoped<IAdministrationService, AdministrationService>();
builder.Services.AddScoped<IListOfValueService, ListOfValueService>();
builder.Services.AddScoped<IApiInternalEsbService, ApiInternalEsbService>();
builder.Services.AddScoped<IApiReportGateway, ApiReportGateway>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<INotiService, NotiService>();



// Cấu hình cookie xác thực
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.LogoutPath = "/Account/Logout";
    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Headers["Accept"].ToString().Contains("application/json"))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            return context.Response.WriteAsync(Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                status = "Error",
                message = "Unauthorized access. Please log in."
            }));
        }
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
});

// Cấu hình HttpClient cho ApiInternalEsbService
builder.Services.AddHttpClient("InternalEsbClient", client =>
{
    var baseAddress = builder.Configuration["VBSPInternalEsbAPI"];
    if (!string.IsNullOrEmpty(baseAddress))
    {
        client.BaseAddress = new Uri(baseAddress);
    }
    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient("NotiGatewayClient", client =>
{
    var baseAddress = builder.Configuration["VBSPNotiGatewayAPI"];
    if (!string.IsNullOrEmpty(baseAddress))
    {
        client.BaseAddress = new Uri(baseAddress);
    }

    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.Add(
        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddScoped<IApiNotiGatewayService, ApiNotiGatewayService>();

// add
builder.Services.AddScoped<IApiInternalService, ApiInternalService>();

builder.Services.AddScoped<INotiService, NotiService>();
builder.Services.AddScoped<IPosRepresentativeService, PosRepresentativeService>();
builder.Services.AddScoped<IUserManagementIDCService, UserManagementIDCService>();
builder.Services.AddControllers().AddControllersAsServices();



builder.Services.AddScoped<IInterestRateConfigureService, InterestRateConfigureService>();

//add cấu hình lãi suất
builder.Services.AddScoped<IProductParameterService,ProductParameterService>();


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//add
// Cấu hình HttpClient cho các API nội bộ
builder.Services.AddHttpClient("InternalApiClient", client =>
{
    var baseAddress = builder.Configuration["VBSPInternalAPI"]; // Sử dụng base address chung
    if (!string.IsNullOrEmpty(baseAddress))
    {
        client.BaseAddress = new Uri(baseAddress);
    }
    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    client.Timeout = TimeSpan.FromSeconds(30);
});
//VBSPInternalAPI

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "RequestVerificationToken";
});

// Cấu hình Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/app.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();
var app = builder.Build();


// ===== CẤU HÌNH CULTURE CHO HỆ THỐNG (FIX DATE dd/MM/yyyy) =====
var culture = new CultureInfo("vi-VN");

// Chuẩn hóa số theo invariant
culture.NumberFormat.NumberDecimalSeparator = ".";
culture.NumberFormat.NumberGroupSeparator = ",";

CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(culture),
    SupportedCultures = new[] { culture },
    SupportedUICultures = new[] { culture }
});

// Middleware xử lý lỗi
app.UseStatusCodePages(async context =>
{
    var response = context.HttpContext.Response;
    if (response.StatusCode == 404)
    {
        response.Redirect("/Home/Forbidden");
    }
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

//app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

//app.UseCors("AllowAll"); // Thêm CORS trước Authentication
app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    await next();
    var logger = context.RequestServices.GetRequiredService<ILoggerFactory>()
                       .CreateLogger("StatusCodeLogger");
    logger.LogInformation("➡️ Path: {Path}, StatusCode: {StatusCode}, Headers: {Headers}",
        context.Request.Path, context.Response.StatusCode, Newtonsoft.Json.JsonConvert.SerializeObject(context.Request.Headers));
});

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();