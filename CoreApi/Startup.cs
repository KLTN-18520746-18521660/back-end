
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Serilog;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;
using DatabaseAccess.Contexts.ConfigDB;
using DatabaseAccess.Contexts.CachedDB;
using DatabaseAccess.Contexts.InventoryDB;
using DatabaseAccess.Contexts.SocialDB;

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
                    // Register Validators -- Will get all validators match with class contained in Assembly 'ConfigDBContext'
                    FluentValidationConfig.RegisterValidatorsFromAssemblyContaining<DBContext>();
                    FluentValidationConfig.RegisterValidatorsFromAssemblyContaining<CachedDBContext>();
                    FluentValidationConfig.RegisterValidatorsFromAssemblyContaining<InventoryDBContext>();
                    FluentValidationConfig.RegisterValidatorsFromAssemblyContaining<SocialDBContext>();
                });
            services.AddDbContext<DBContext>();
            services.AddDbContext<CachedDBContext>();
            services.AddDbContext<InventoryDBContext>();
            services.AddDbContext<SocialDBContext>();
            services.AddSwaggerGen(c => {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "CoreApi", Version = "v1" });
            });
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
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CoreApi v1"));
                
                __Logger.Warning("Application is running in development mode will include: swagger");
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
