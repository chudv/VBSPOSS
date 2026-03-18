//using Microsoft.AspNetCore.Authentication.Cookies;
//using Microsoft.AspNetCore.Authentication;
//using Microsoft.AspNetCore.Localization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.IdentityModel.Logging;
//using Newtonsoft.Json.Serialization;
//using Serilog;
//using System.Globalization;
//using System.IdentityModel.Tokens.Jwt;
//using VBSPOSS.Data;
//using VBSPOSS.Services.Implement;
//using VBSPOSS.Services.Interface;
//using Microsoft.AspNetCore.Identity;
//using VBSPOSS.Areas.Identity.Data;
//using VBSPOSS.Mappings;

//namespace VBSPOSS
//{
//    public class Startup
//    {
//        public Startup(IConfiguration configuration)
//        {
//            //JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
//            Configuration = configuration;           
//            // Cấu hình Serilog
//            Log.Logger = new LoggerConfiguration()
//                .WriteTo.Console() // Log ra màn hình Console
//                .WriteTo.File("logs/app.log", rollingInterval: RollingInterval.Day) // Lưu log vào file
//                                                                                    //.ReadFrom.Configuration(builder.Configuration) // Đọc config từ appsettings.json
//                .CreateLogger();
            
//        }

//        public IConfiguration Configuration { get; }

//        // This method gets called by the runtime. Use this method to add services to the container.
//        public void ConfigureServices(IServiceCollection services)
//        {
//            IdentityModelEventSource.ShowPII = true;

//            //var rootConfiguration = CreateRootConfiguration();
//            //services.AddSingleton(rootConfiguration);

//            var connectionString = Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

//            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
//            services.AddDbContext<MyIdentityDbContext>(options => options.UseSqlServer(connectionString));
//            services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<MyIdentityDbContext>();

//            // Add services to the container.
//            services.AddControllersWithViews();
//            services.AddMvc().AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);
//            services.AddKendo();
//            services.AddAutoMapper(typeof(MappingProfile));            
//            services.AddSingleton<IConfiguration>(Configuration);            
//            //AddAuthenticationServices(services, rootConfiguration.AdminConfiguration);

            
//            services.AddSession(options =>
//            {
//                options.IdleTimeout = TimeSpan.FromMinutes(5);
//                //options.Cookie.HttpOnly = true;
//                //options.Cookie.IsEssential = true;
//            });

//            services.AddMvc(options => options.EnableEndpointRouting = false);

//            services.ConfigureApplicationCookie(options =>
//            {
//                options.ExpireTimeSpan = System.TimeSpan.FromMinutes(5);
//                options.SlidingExpiration = true;
//                options.LoginPath = "/";
//            });
                       
//            //RegisterAuthorization(services, rootConfiguration);
//            services.AddControllersWithViews();
//            services.AddSingleton<ISessionHelper, SessionHelper>();
//            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();            
//            services.AddScoped<IAdministrationService, AdministrationService>();

//            services.AddScoped<IListOfValueService, ListOfValueService>();

//            //Add xử lý cho phần giao tiếp với API Admin
//            services.AddHttpContextAccessor();

            

//        }

//        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
//        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
//        {
//            //Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("ODc4NkAzMjMwMkUzNDJFMzBsa2MvT0xqRTVEaHV1d01nNjUveFFoV2dWbHhhTVBIWVZ4alJjS3ltaVZnPQ==");

//            if (env.IsDevelopment())
//            {
//                app.UseDeveloperExceptionPage();
//            }
//            else
//            {
//                app.UseExceptionHandler("/Home/Error");
//                //The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
//                app.UseHsts();
//            }

//            var supportedCultures = new[] { new CultureInfo("vi-VN") };
//            app.UseRequestLocalization(new RequestLocalizationOptions
//            {
//                DefaultRequestCulture = new RequestCulture("vi-VN"),
//                SupportedCultures = supportedCultures,
//                SupportedUICultures = supportedCultures
//            });

//            loggerFactory.AddSerilog();
//            //app.UseHttpsRedirection();
//            app.UseStaticFiles();
//            app.UseRouting();
//            app.UseAuthentication();
//            app.UseAuthorization();
//            app.UseSession();
//            //app.MapRazorPages();
//            app.UseEndpoints(endpoints =>
//            {
//                endpoints.MapControllers();
//            });

//            //app.UseEndpoints(endpoints =>
//            //{
//            //    endpoints.MapControllerRoute(
//            //        name: "default",
//            //        pattern: "{controller=Home}/{action=Index}/{id?}");
//            //});

//            //app.UseMvc(routes =>
//            //{
//            //    routes.MapRoute(
//            //        name: "default",
//            //        template: "{controller=Home}/{action=Index}/{id?}");
//            //});

//        }

//        //protected IRootConfiguration CreateRootConfiguration()
//        //{
//        //    var rootConfiguration = new RootConfiguration();
//        //    Configuration.GetSection(ConfigurationConsts.AdminConfigurationKey).Bind(rootConfiguration.AdminConfiguration);
//        //    return rootConfiguration;
//        //}

//        //private void AddAuthenticationServices(IServiceCollection services, AdminConfiguration adminConfiguration)
//        //{
//        //    services.AddAuthentication(options =>
//        //    {
//        //        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//        //        options.DefaultChallengeScheme = AuthenticationConsts.OidcAuthenticationScheme;
//        //        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//        //        options.DefaultForbidScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//        //        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//        //        options.DefaultSignOutScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//        //    })
//        //            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme,
//        //                options =>
//        //                {
//        //                    options.Cookie.Name = adminConfiguration.IdentityAdminCookieName;
//        //                    options.Cookie.SameSite = SameSiteMode.None;
//        //                    //options.Cookie.SecurePolicy = CookieSecurePolicy.
//        //                })

//        //            .AddOpenIdConnect(AuthenticationConsts.OidcAuthenticationScheme, options =>
//        //            {
//        //                options.Authority = adminConfiguration.IdentityServerBaseUrl;
//        //                options.RequireHttpsMetadata = adminConfiguration.RequireHttpsMetadata;
//        //                options.ClientId = adminConfiguration.ClientId;
//        //                options.ClientSecret = adminConfiguration.ClientSecret;
//        //                options.ResponseType = adminConfiguration.OidcResponseType;
//        //                options.Scope.Clear();
//        //                foreach (var scope in adminConfiguration.Scopes)
//        //                {
//        //                    options.Scope.Add(scope);
//        //                }

//        //                options.ClaimActions.MapJsonKey(adminConfiguration.TokenValidationClaimRole, adminConfiguration.TokenValidationClaimRole, adminConfiguration.TokenValidationClaimRole);
//        //                options.SaveTokens = true;
//        //                options.GetClaimsFromUserInfoEndpoint = true;
//        //                options.TokenValidationParameters = new TokenValidationParameters
//        //                {
//        //                    NameClaimType = adminConfiguration.TokenValidationClaimName,
//        //                    RoleClaimType = adminConfiguration.TokenValidationClaimRole
//        //                };

//        //                options.Events = new OpenIdConnectEvents
//        //                {
//        //                    OnMessageReceived = context => OnMessageReceived(context, adminConfiguration),
//        //                    OnRedirectToIdentityProvider = context => OnRedirectToIdentityProvider(context, adminConfiguration),
//        //                    OnAuthenticationFailed = context =>
//        //                    {
//        //                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
//        //                        {
//        //                            context.Response.Headers.Add("Token-Expired", "true");
//        //                        }
//        //                        return Task.CompletedTask;
//        //                    }
//        //                };
//        //            });
//        //}

//        //private Task OnMessageReceived(MessageReceivedContext context, AdminConfiguration adminConfiguration)
//        //{
//        //    context.Properties.IsPersistent = true;
//        //    context.Properties.ExpiresUtc = new DateTimeOffset(DateTime.Now.AddHours(adminConfiguration.IdentityAdminCookieExpiresUtcHours));
//        //    return Task.FromResult(0);
//        //}

//        //private Task OnRedirectToIdentityProvider(RedirectContext n, AdminConfiguration adminConfiguration)
//        //{
//        //    n.ProtocolMessage.RedirectUri = adminConfiguration.IdentityAdminRedirectUri;
//        //    return Task.FromResult(0);
//        //}

//        //private void RegisterAuthorization(IServiceCollection services, IRootConfiguration rootConfiguration)
//        //{

//        //}
//    }
//}
