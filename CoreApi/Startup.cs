
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Serilog;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;
using DatabaseAccess.Context;
using DatabaseAccess;
using Microsoft.AspNetCore.Mvc;
using CoreApi.Common;
using CoreApi.Services;
using System;
using System.Reflection;
using System.IO;
using System.ComponentModel;

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

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddControllers()
                .AddNewtonsoftJson()
                .AddFluentValidation(FluentValidationConfig =>
                {
                    // Dot not use base validate with Annotation
                    FluentValidationConfig.DisableDataAnnotationsValidation = true;
                    // Register Validators -- Will get all validators match with class contained in Assembly 'DBContext'
                    FluentValidationConfig.RegisterValidatorsFromAssemblyContaining<DBContext>();
                    FluentValidationConfig.RegisterValidatorsFromAssemblyContaining<Startup>();
                });
            services.AddDbContext<DBContext>();
            services.AddSingleton<BaseConfig, BaseConfig>();
            services.AddSingleton<AdminAuditLogManagement, AdminAuditLogManagement>();
            services.AddSingleton<AdminUserManagement, AdminUserManagement>();
            services.AddSingleton<SessionAdminUserManagement, SessionAdminUserManagement>();
#if DEBUG
            services.AddMvcCore(options => {
                options.Filters.Add<ValidatorFilter>();
                options.Conventions.Add(new ApiExplorerConvention());
            });
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });
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
                c.SwaggerDoc("test", new OpenApiInfo {
                    Title = "CoreApi",
                    Version = "v1",
                    Description = "Api testing blog. (KLTN-UIT-18520746-18521660)",
                    Contact = new OpenApiContact {
                        Name = "KLTN-UIT-18520746-18521660",
                        Email = "18520746@gm.uit.edu.vn",
                        Url = new Uri("https://github.com/KLTN-18520746-18521660/back-end"),
                    },
                });
                c.OperationFilter<HeadersFilter>();
                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
#endif
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, 
            IWebHostEnvironment env,
            IHostApplicationLifetime hostApplicationLifetime)
        {
            hostApplicationLifetime.ApplicationStopping.Register(OnStopping);
            hostApplicationLifetime.ApplicationStarted.Register(OnStarted);

            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
                app.UseSwagger(c => {
                    // c.SerializeAsV2 = true;
                });
                app.UseSwaggerUI(c => {
                    c.SwaggerEndpoint("/swagger/admin/swagger.json", "CoreApi - Admin");
                    c.SwaggerEndpoint("/swagger/social/swagger.json", "CoreApi - Social");
                    c.SwaggerEndpoint("/swagger/test/swagger.json", "CoreApi - Testing");
                    
                    c.RoutePrefix = string.Empty;
                });

                __Logger.Warning($"Application is running in development mode will include: swagger.");
            }

            app.UseRouting();
            app.UseAuthorization();
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
