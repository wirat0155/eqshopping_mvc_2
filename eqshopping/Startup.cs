using eqshopping.Data;
using eqshopping.Middlewares;
using eqshopping.Repositories;
using eqshopping.Utility;
using JWTRegen.Interfaces;
using JWTRegen.Middleware;
using JWTRegen.Models;
using JWTRegen.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Starter.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eqshopping
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            #region Selilog (Hour Error Log)
            var logger = new LoggerConfiguration()
              .WriteTo.File("wwwroot/logs/Logger.txt", rollingInterval: RollingInterval.Hour)
              .MinimumLevel.Error()
              .CreateLogger();
            services.AddSerilog(logger);
            #endregion
            #region CORS
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAnyOrigin", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyHeader()
                           .AllowAnyMethod();
                });
            });
            #endregion
            #region JWT
            services.AddScoped<JWTRegen.Interfaces.IJwtTokenService, JWTRegen.Services.JwtTokenService>();
            services.AddScoped<JWTRegen.Interfaces.IClaimsHelper, JWTRegen.Services.ClaimsHelper>();
            var jwtSettingsSection = Configuration.GetSection("JwtSettings");
            services.Configure<JwtSettings>(jwtSettingsSection);

            var jwtSettings = jwtSettingsSection.Get<JwtSettings>();
            var key = Encoding.ASCII.GetBytes(jwtSettings.SecretKey);

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;  // Enable in production (false for local testing)
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
                x.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var token = context.Request.Cookies["eqshopping_jwt"];
                        if (!string.IsNullOrEmpty(token))
                        {
                            context.Token = token;
                        }
                        return Task.CompletedTask;
                    }
                };
            });
            #endregion
            services.AddAutoMapper(typeof(Startup));
            services.AddControllersWithViews();
            services.AddSingleton<IConfiguration>(Configuration);
            #region DB
            services.AddDbContext<UICT2EQSDbContext>(options =>
            options.UseSqlServer(
                Configuration.GetConnectionString("UICT2_EQS"),
                sqlServerOptions =>
                {
                    sqlServerOptions.EnableRetryOnFailure();
                }).EnableSensitiveDataLogging()
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

            services.AddDbContext<UICTDbContext>(options =>
            options.UseSqlServer(
                Configuration.GetConnectionString("UICT"),
                sqlServerOptions =>
                {
                    sqlServerOptions.EnableRetryOnFailure();
                }).EnableSensitiveDataLogging()
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));


            services.AddDistributedMemoryCache();
            services.AddSession(options => {
                options.IdleTimeout = TimeSpan.FromHours(13);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
            #endregion
            #region Services
            services.AddScoped(typeof(DropdownUtility));
            services.AddScoped(typeof(DapperService));
            services.AddScoped(typeof(UserManagementRepository));
            services.AddScoped(typeof(PdCellRepository));
            services.AddScoped(typeof(CellLockRepository));
            services.AddScoped(typeof(ProductionOrderRepository));
            services.AddScoped(typeof(CellProductRepository));
            services.AddScoped(typeof(ProductEquipmentRepository));
            services.AddScoped(typeof(ShoppingTranRepository));
            services.AddScoped(typeof(ShoppingTranSubRepository));
            services.AddScoped(typeof(ProductRepository));
            services.AddScoped(typeof(AuthRepository));
            services.AddScoped(typeof(EmployeeRepository));
            services.AddScoped(typeof(UserMenuRepository));
            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.Use(async (context, next) =>
            {
                context.Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
                context.Response.Headers["Pragma"] = "no-cache";
                context.Response.Headers["Expires"] = "0";

                await next();
            });

            app.Use(async (context, next) =>
            {
                try
                {
                    await next.Invoke();
                }
                catch (Exception ex)
                {
                    // Check if the exception is from token validation failure
                    if (ex.Message.Contains("Token has expired"))
                    {
                        // Clear the token cookie safely
                        if (context.Request.Cookies.ContainsKey("eqshopping_jwt"))
                        {
                            context.Response.Cookies.Delete("eqshopping_jwt");
                        }

                        // Redirect to the login page
                        context.Response.Redirect("/Auth/vLogin");
                    }
                    else
                    {
                        // Re-throw other exceptions
                        throw;
                    }
                }
            });


            // JWT and redirect middlewares
            app.UseMiddleware<JWTRegen.Middleware.TokenVersionMiddleware>();
            app.UseMiddleware<RedirectUnauthorizedMiddleware>();


            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseMiddleware<JWTRegen.Middleware.TokenVersionMiddleware>();
            app.UseMiddleware<RedirectUnauthorizedMiddleware>();

            app.UseCors("AllowAnyOrigin");
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Auth}/{action=vLogin}/{id?}");
            });
        }
    }
}