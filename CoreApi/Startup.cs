using CoreApi.Common;
using CoreApi.Services;
using CoreApi.Services.Background;
using DatabaseAccess;
using DatabaseAccess.Context;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Threading.Channels;

namespace CoreApi
{
    public class Startup
    {
        private readonly ILogger __Logger;
        public IConfiguration __Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            __Logger = Log.Logger;
            __Configuration = configuration;
        }

        // [INFO] This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddControllers()
                .AddNewtonsoftJson()
                .AddFluentValidation(FluentValidationConfig =>
                {
                    // [INFO] Dot not use base validate with Annotation
                    FluentValidationConfig.DisableDataAnnotationsValidation = true;
                    // Register Validators -- Will get all validators match with class contained in Assembly
                    FluentValidationConfig.RegisterValidatorsFromAssemblyContaining<DBContext>();
                    FluentValidationConfig.RegisterValidatorsFromAssemblyContaining<Startup>();
                });
            #region Add email client
            services
                .AddFluentEmail(Program.EmailClientConfig.User)
                .AddRazorRenderer()
                .AddSmtpSender(new SmtpClient(){
                    Host = Program.EmailClientConfig.Host,
                    Port = int.Parse(Program.EmailClientConfig.Port),
                    EnableSsl = Program.EmailClientConfig.EnableSSL,
                    Credentials = new NetworkCredential(Program.EmailClientConfig.User, Program.EmailClientConfig.Password),
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                });
            #endregion
            #region Add services
            // [IMPORTANT] Only inject DBContext to controller when necessary, must use define service instead of DBContext
            services
                .AddDbContext<DBContext>(o => {
                    o.LogTo((string msg) => {
                        if (msg.Contains("Microsoft.EntityFrameworkCore.Database.Command")) {
                            return;
                        }
                        string level = msg.TrimStart().Substring(0, 1);
                        string realMsg = string.Join(" ", msg.Split("->").Skip(1).ToArray()).Trim();
                        if (level.StartsWith("i", true, null)) {
                            __Logger.Information(realMsg);
                        } else if (level.StartsWith("w", true, null)) {
                            __Logger.Warning(realMsg);
                        } else {
                            __Logger.Error(realMsg);
                        }
                    }, Microsoft.Extensions.Logging.LogLevel.Information, DbContextLoggerOptions.Category | DbContextLoggerOptions.Level | DbContextLoggerOptions.SingleLine);
                }, ServiceLifetime.Singleton);
            // Defind services
            services.AddHostedService<EmailDispatcher>();
            services.AddSingleton(Channel.CreateUnbounded<EmailChannel>())
                    .AddSingleton<BaseConfig>()
                    .AddSingleton<EmailSender>()
                    .AddTransient<AdminUserManagement>()
                    .AddTransient<AdminAuditLogManagement>()
                    .AddTransient<SessionAdminUserManagement>()
                    .AddTransient<SocialPostManagement>()
                    .AddTransient<SocialUserManagement>()
                    .AddTransient<SocialUserAuditLogManagement>()
                    .AddTransient<SocialAuditLogManagement>()
                    .AddTransient<SessionSocialUserManagement>();
            #endregion

            services
                .AddMvcCore(options => {
                    options.Filters.Add<ValidatorFilter>();
                    options.Conventions.Add(new ApiExplorerConvention());
                })
                .ConfigureApiBehaviorOptions(options => {
                    options.SuppressMapClientErrors = true;
                    options.SuppressModelStateInvalidFilter = true;
                });
            #region Config Swagger document
            services.AddSwaggerGen(c => {
                c.SwaggerDoc("admin", new OpenApiInfo {
                    Title = "CoreApi",
                    Version = "v1",
                    Description = "CoreApi for manage blog. (KLTN-UIT-18520746-18521660)",
                    Contact = new OpenApiContact {
                        Name = "KLTN-UIT-18520746-18521660",
                        Email = "18520746@gm.uit.edu.vn",
                        Url = new Uri("https://github.com/KLTN-18520746-18521660/back-end"),
                    },
                });
                c.SwaggerDoc("social", new OpenApiInfo {
                    Title = "CoreApi",
                    Version = "v1",
                    Description = "CoreApi for using blog. (KLTN-UIT-18520746-18521660)",
                    Contact = new OpenApiContact {
                        Name = "KLTN-UIT-18520746-18521660",
                        Email = "18520746@gm.uit.edu.vn",
                        Url = new Uri("https://github.com/KLTN-18520746-18521660/back-end"),
                    },
                });
                c.SwaggerDoc("testing", new OpenApiInfo {
                    Title = "CoreApi",
                    Version = "v1",
                    Description = "Api testing blog. (KLTN-UIT-18520746-18521660)",
                    Contact = new OpenApiContact {
                        Name = "KLTN-UIT-18520746-18521660",
                        Email = "18520746@gm.uit.edu.vn",
                        Url = new Uri("https://github.com/KLTN-18520746-18521660/back-end"),
                    },
                });
                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
            #endregion

            services.AddCors();
        }

        // [INFO] This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app,
            IWebHostEnvironment env,
            IHostApplicationLifetime hostApplicationLifetime)
        {
            hostApplicationLifetime.ApplicationStopping.Register(OnStopping);
            hostApplicationLifetime.ApplicationStarted.Register(OnStarted);

            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
                __Logger.Warning($"Application is running in development mode.");
            }

            if (Program.EnableSwagger || env.IsDevelopment()) {
                app.UseSwagger(c => {
                    c.SerializeAsV2 = true;
                });
                app.UseSwaggerUI(c => {
                    c.SwaggerEndpoint("/swagger/admin/swagger.json", "CoreApi - Admin");
                    c.SwaggerEndpoint("/swagger/social/swagger.json", "CoreApi - Social");
                    c.SwaggerEndpoint("/swagger/testing/swagger.json", "CoreApi - Testing");

                    c.RoutePrefix = string.Empty;
                });
            }

            if (app.ApplicationServices.GetService<DBContext>().GetStatus()) {
                __Logger.Information($"Connected to database, host: { BaseConfigurationDB.Host }, port: { BaseConfigurationDB.Port }");
            } else {
                throw new Exception($"Failed to connect to database, host: { BaseConfigurationDB.Host }, port: { BaseConfigurationDB.Port }");
            }
            app.ApplicationServices.GetService<BaseConfig>();
            app.ApplicationServices.GetService<EmailSender>();

            app.UseRouting();
            app.UseCors(builder =>
            {
                builder.SetIsOriginAllowedToAllowWildcardSubdomains();
                builder.SetIsOriginAllowed((origin) =>
                {
                    return
                        Program.AllowAnyOrigin ||
                        Program.ListeningAddress.Contains(origin) ||
                        Program.HostName == origin;
                });
                builder.WithMethods(Program.AllowMethods.ToArray());
                builder.WithHeaders(Program.AllowHeaders.ToArray());
            });
            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });
        }
        private void OnStarted()
        {
            __Logger.Information("Host is strated");
        }
        private void OnStopping()
        {
            __Logger.Information("Host is stopping...");
        }
    }
}
