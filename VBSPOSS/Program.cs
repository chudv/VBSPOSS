using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Globalization;
using System.Net.Http.Headers;
using VBSPOSS.Areas.Identity.Data;
using VBSPOSS.Data;
using VBSPOSS.Helpers.Implements;
using VBSPOSS.Helpers.Interfaces;
using VBSPOSS.Implements.Helpers;
using VBSPOSS.Integration.Implements;
using VBSPOSS.Integration.Interfaces;
using VBSPOSS.Mappings;
using VBSPOSS.Services.Implements;
using VBSPOSS.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IEncryptionService, AesEncryptionService>();
builder.Services.AddSingleton<IConnectionStringProvider, ConnectionStringProvider>();

// ========================= DB CONTEXT =========================

builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
{
    var provider = sp.GetRequiredService<IConnectionStringProvider>();
    var connStr = provider.GetOSSConnectionString();

    options.UseSqlServer(connStr);
});

builder.Services.AddDbContext<MyIdentityDbContext>((sp, options) =>
{
    var provider = sp.GetRequiredService<IConnectionStringProvider>();
    var connStr = provider.GetOSSConnectionString();

    options.UseSqlServer(connStr);
});

builder.Services.AddDbContext<IntellectIDCDbContext>((sp, options) =>
{
    var provider = sp.GetRequiredService<IConnectionStringProvider>();
    var connStr = provider.GetOracleConnectionString();

    options.UseOracle(connStr);
});

// ========================= IDENTITY =========================

builder.Services
    .AddDefaultIdentity<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<MyIdentityDbContext>();

// ========================= MVC =========================

builder.Services
    .AddControllersWithViews()
    .AddRazorRuntimeCompilation();

builder.Services
    .AddMvc()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.PropertyNamingPolicy = null);

builder.Services.AddControllers().AddControllersAsServices();

builder.Services.AddKendo();
builder.Services.AddAutoMapper(typeof(MappingProfile));

// ========================= SESSION =========================

builder.Services.AddSession(options =>
{
    // Timeout session sau 30 phút không hoạt động
    options.IdleTimeout = TimeSpan.FromMinutes(30);

    options.Cookie.Name = ".VBSPOSS.Session";

    options.Cookie.HttpOnly = true;

    // Chỉ gửi cookie qua HTTPS nếu môi trường có SSL
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;

    options.Cookie.IsEssential = true;
});

// ========================= COOKIE AUTH =========================

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";

    options.AccessDeniedPath = "/Account/AccessDenied";

    options.LogoutPath = "/Account/Logout";

    // Hết hạn sau 30 phút
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);

    // TRUE = user hoạt động liên tục sẽ được gia hạn
    // FALSE = đúng 30 phút là logout
    options.SlidingExpiration = true;

    options.Cookie.HttpOnly = true;

    options.Cookie.Name = ".VBSPOSS.Auth";

    options.Events.OnRedirectToLogin = context =>
    {
        // Request AJAX/API
        if (context.Request.Headers["Accept"]
            .ToString()
            .Contains("application/json"))
        {
            context.Response.StatusCode =
                StatusCodes.Status401Unauthorized;

            context.Response.ContentType = "application/json";

            return context.Response.WriteAsync(
                Newtonsoft.Json.JsonConvert.SerializeObject(new
                {
                    status = "Error",
                    message = "Session expired. Please login again."
                }));
        }

        context.Response.Redirect(context.RedirectUri);

        return Task.CompletedTask;
    };
});

// ========================= HTTP CONTEXT =========================

builder.Services.AddHttpContextAccessor();

builder.Services.AddSingleton<IHttpContextAccessor,
    HttpContextAccessor>();

builder.Services.AddSingleton<ISessionHelper,
    SessionHelper>();

// ========================= SERVICES =========================

builder.Services.AddScoped<IProductService,
    ProductService>();

builder.Services.AddScoped<IAdministrationService,
    AdministrationService>();

builder.Services.AddScoped<IListOfValueService,
    ListOfValueService>();

builder.Services.AddScoped<IApiInternalEsbService,
    ApiInternalEsbService>();

builder.Services.AddScoped<IApiReportGateway,
    ApiReportGateway>();

builder.Services.AddScoped<IReportService,
    ReportService>();

builder.Services.AddScoped<INotiService,
    NotiService>();

builder.Services.AddScoped<IListOfTransPointService,
    ListOfTransPointService>();

builder.Services.AddScoped<IAttachedFileService,
    AttachedFileService>();

builder.Services.AddScoped<ITransferDataPosService,
    TransferDataPosService>();

builder.Services.AddScoped<IApiNotiGatewayService,
    ApiNotiGatewayService>();

builder.Services.AddScoped<IApiInternalService,
    ApiInternalService>();

builder.Services.AddScoped<IPosRepresentativeService,
    PosRepresentativeService>();

builder.Services.AddScoped<IListOfCommuneService,
    ListOfCommuneService>();

builder.Services.AddScoped<IUserManagementIDCService,
    UserManagementIDCService>();

builder.Services.AddScoped<IScriptExecutionService,
    ScriptExecutionService>();

builder.Services.AddScoped<IInterestRateConfigureService,
    InterestRateConfigureService>();

builder.Services.AddScoped<IProductParameterService,
    ProductParameterService>();

// ========================= HTTP CLIENT =========================

builder.Services.AddHttpClient("InternalEsbClient", client =>
{
    var baseAddress =
        builder.Configuration["VBSPInternalEsbAPI"];

    if (!string.IsNullOrEmpty(baseAddress))
    {
        client.BaseAddress = new Uri(baseAddress);
    }

    client.DefaultRequestHeaders.Accept.Clear();

    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));

    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient("NotiGatewayClient", client =>
{
    var baseAddress =
        builder.Configuration["VBSPNotiGatewayAPI"];

    if (!string.IsNullOrEmpty(baseAddress))
    {
        client.BaseAddress = new Uri(baseAddress);
    }

    client.DefaultRequestHeaders.Accept.Clear();

    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));

    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient("InternalApiClient", client =>
{
    var baseAddress =
        builder.Configuration["VBSPInternalAPI"];

    if (!string.IsNullOrEmpty(baseAddress))
    {
        client.BaseAddress = new Uri(baseAddress);
    }

    client.DefaultRequestHeaders.Accept.Clear();

    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));

    client.Timeout = TimeSpan.FromSeconds(30);
});

// ========================= ANTIFORGERY =========================

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "RequestVerificationToken";
});

// ========================= SERILOG =========================

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/app.log",
        rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

var app = builder.Build();

// ========================= CULTURE =========================

var culture = new CultureInfo("vi-VN");

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

// ========================= ERROR =========================

app.UseStatusCodePages(async context =>
{
    var response = context.HttpContext.Response;

    if (response.StatusCode == 404)
    {
        response.Redirect("/Home/Forbidden");
    }
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");

    app.UseHsts();
}

// ========================= PIPELINE =========================

//app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

// QUAN TRỌNG:
// Session phải đứng trước Authentication

app.UseSession();

app.UseAuthentication();

app.UseAuthorization();

// LOG STATUS CODE

app.Use(async (context, next) =>
{
    await next();

    var logger = context.RequestServices
        .GetRequiredService<ILoggerFactory>()
        .CreateLogger("StatusCodeLogger");

    logger.LogInformation(
        "➡️ Path: {Path}, StatusCode: {StatusCode}",
        context.Request.Path,
        context.Response.StatusCode);
});

// ========================= ROUTING =========================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();