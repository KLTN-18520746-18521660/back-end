using CoreApi.Common;
using CoreApi.Common.Base;
using CoreApi.Common.Middlerware;
using CoreApi.Services;
using CoreApi.Services.Background;
using DatabaseAccess;
using DatabaseAccess.Context;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
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
            // [INFO] Handle https
            if (Program.ServerConfiguration.EnableSSL) {
                services.AddHttpsRedirection(options => {
                    options.HttpsPort = Program.ServerConfiguration.SslPort;
                });
            }

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
                        if (!Program.ServerConfiguration.ShowSQLCommandInLog && msg.Contains("Microsoft.EntityFrameworkCore.Database.Command")) {
                            return;
                        }
                        string level = msg.TrimStart().Substring(0, 1);
                        string realMsg = string.Join(" ", msg.Split("->").Skip(1).ToArray()).Trim();
                        if (level.StartsWith("i", true, default)) {
                            __Logger.Information(realMsg);
                        } else if (level.StartsWith("w", true, default)) {
                            __Logger.Warning(realMsg);
                        } else {
                            __Logger.Error(realMsg);
                        }
                    }, Microsoft.Extensions.Logging.LogLevel.Information, DbContextLoggerOptions.Category | DbContextLoggerOptions.Level | DbContextLoggerOptions.SingleLine);
                }, ServiceLifetime.Transient);
            // Defind services
            services.AddHostedService<EmailDispatcher>();
            services.AddSingleton(Channel.CreateUnbounded<EmailChannel>())
                    .AddSingleton<BaseConfig>()
                    .AddSingleton<EmailSender>()
                    .AddSingleton<NotificationsManagement>()
                    .AddTransient<AdminUserManagement>()
                    .AddTransient<AdminAuditLogManagement>()
                    .AddTransient<SessionAdminUserManagement>()
                    .AddTransient<SocialPostManagement>()
                    .AddTransient<SocialCategoryManagement>()
                    .AddTransient<SocialCommentManagement>()
                    .AddTransient<SocialReportManagement>()
                    .AddTransient<SocialTagManagement>()
                    .AddTransient<SocialUserManagement>()
                    .AddTransient<SocialUserAuditLogManagement>()
                    .AddTransient<SocialAuditLogManagement>()
                    .AddTransient<RedirectUrlManagement>()
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
                c.SwaggerDoc("upload", new OpenApiInfo {
                    Title = "CoreApi",
                    Version = "v1",
                    Description = "CoreApi for upload file. (KLTN-UIT-18520746-18521660)",
                    Contact = new OpenApiContact {
                        Name = "KLTN-UIT-18520746-18521660",
                        Email = "18520746@gm.uit.edu.vn",
                        Url = new Uri("https://github.com/KLTN-18520746-18521660/back-end"),
                    },
                });
#if DEBUG
                c.SwaggerDoc("test", new OpenApiInfo {
                    Title = "CoreApi",
                    Version = "v1",
                    Description = "Api test blog. (KLTN-UIT-18520746-18521660)",
                    Contact = new OpenApiContact {
                        Name = "KLTN-UIT-18520746-18521660",
                        Email = "18520746@gm.uit.edu.vn",
                        Url = new Uri("https://github.com/KLTN-18520746-18521660/back-end"),
                    },
                });
#endif
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

            if (Program.ServerConfiguration.EnableSSL) {
                app.UseHttpsRedirection();
            }

            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
                __Logger.Warning($"Application is running in development environment.");
            }

            if (Program.SwaggerDocumentConfiguration.Enable || env.IsDevelopment()) {
                app.UseAuthentication(); //Ensure this like is above the swagger stuff

                app.UseSwaggerAuthorized();
                app.UseHandleTrimBodyRequest();
                app.UseSwagger(c => {
                    c.SerializeAsV2 = true;
                });
                app.UseSwaggerUI(c => {
                    c.SwaggerEndpoint("/swagger/social/swagger.json", "CoreApi - Social");
                    c.SwaggerEndpoint("/swagger/admin/swagger.json", "CoreApi - Admin");
                    c.SwaggerEndpoint("/swagger/upload/swagger.json", "CoreApi - Upload");
#if DEBUG
                    c.SwaggerEndpoint("/swagger/test/swagger.json", "CoreApi - Testing");
#endif

                    c.DefaultModelsExpandDepth(-1);
                    c.RoutePrefix = Program.SwaggerDocumentConfiguration.Path.Remove(0, 1);
                });
            }
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                var __DBContext = serviceScope.ServiceProvider.GetService<DBContext>();
                if (Program.DropDatabase) {
                    __Logger.Warning($"Dropping database.host: { BaseConfigurationDB.Host }, port: { BaseConfigurationDB.Port }, db_name: { BaseConfigurationDB.DBName }");
                    __DBContext.Database.EnsureDeleted();
                }
                if (Program.RunWithoutMigrateDatabase) {
                    __Logger.Warning($"Application is running without migrate database");
                } else {
                    __DBContext.Database.Migrate();
                }
                if (__DBContext.GetStatus()) {
                    __Logger.Information($"Connected to database, host: { BaseConfigurationDB.Host }, port: { BaseConfigurationDB.Port }, db_name: { BaseConfigurationDB.DBName }");
                } else {
                    throw new Exception($"Failed to connect to database, host: { BaseConfigurationDB.Host }, port: { BaseConfigurationDB.Port }, db_name: { BaseConfigurationDB.DBName }");
                }

                serviceScope.ServiceProvider.GetService<SocialUserManagement>().UpdateDefaultSocialRole();
                serviceScope.ServiceProvider.GetService<AdminUserManagement>().UpdateDefaultAdminRole();
            }
            app.ApplicationServices.GetService<BaseConfig>();
            app.ApplicationServices.GetService<EmailSender>();

            app.UseRouting();
            app.UseCors(builder =>
            {
                if (Program.ServerConfiguration.DisableCORS) {
                    builder.AllowAnyHeader();
                    builder.AllowAnyMethod();
                    builder.AllowAnyOrigin();
                } else {
                    builder.SetIsOriginAllowedToAllowWildcardSubdomains();
                    builder.SetIsOriginAllowed((origin) =>
                    {
                        return
                            Program.ListeningAddress.Contains(origin) ||
                            Program.ServerConfiguration.HostName == origin;
                    });
                    builder.WithMethods(Program.AllowMethods.ToArray());
                    builder.WithHeaders(Program.AllowHeaders.ToArray());
                }
            });

            app.Use(async (context, next) => {
                await next();
                if (context.Request.Path.Value != default
                    && (context.Response.StatusCode == 404
                        || context.Response.StatusCode == 405)
                    && !Path.HasExtension(context.Request.Path.Value)
                    && !context.Request.Path.Value.StartsWith("/api")
#if DEBUG
                    && !context.Request.Path.Value.StartsWith(Program.ServerConfiguration.PrefixPathGetUploadFile)
#endif
                ) {
                    context.Request.Path = "/index.html";
                    await next();
                }
            });

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });

            var uploadFileProvider = new PhysicalFileProvider(Program.ServerConfiguration.UploadFilePath);
            var wellKnownFileProvider = new PhysicalFileProvider(Program.ServerConfiguration.WellKnownFilePath);
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = uploadFileProvider,
                RequestPath = Program.ServerConfiguration.PrefixPathGetUploadFile
            });
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = wellKnownFileProvider,
                RequestPath = Program.ServerConfiguration.PrefixPathGetWellKnownFile
            });
#if DEBUG
            app.UseDirectoryBrowser(new DirectoryBrowserOptions
            {
                FileProvider = uploadFileProvider,
                RequestPath = Program.ServerConfiguration.PrefixPathGetUploadFile
            });
            app.UseDirectoryBrowser(new DirectoryBrowserOptions
            {
                FileProvider = wellKnownFileProvider,
                RequestPath = Program.ServerConfiguration.PrefixPathGetUploadFile
            });
#endif
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
