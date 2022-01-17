using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace core_api
{
    public static class ConfigurationDefaultVariable
    {
        public static readonly int PORT = 7005;
        // Password default of certificate
        public static readonly string PASSWORD = "Ndh90768";
        public static readonly string TMP_PATH = "./tmp/";
        public static readonly string LOG_PATH = "./log/core-api.log";
    }

    public class Program
    {
        private static int ApiPort;
        private static string CertPath;
        private static string TmpPath = core_api.ConfigurationDefaultVariable.TMP_PATH;
        private static string LogPath = core_api.ConfigurationDefaultVariable.LOG_PATH;
        public static int Port { get =>- ApiPort; }
        
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            var loggerFactory = LoggerFactory.Create(
                builder => 
                {
                    builder
                        .AddFilter("Microsoft", LogLevel.Warning)
                        .AddFilter("System", LogLevel.Warning)
                        .AddFilter("YourProgramNamespsace.Program", LogLevel.Debug)
                        .AddConsole();
                });
            var logger = loggerFactory.CreateLogger<Program>();
            try 
            {
                host.Run();
            } 
            catch (Exception e) 
            {
                logger.LogError(e.ToString());
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, service) => 
                {
                    try 
                    {
                        // [INFO] Config custom port from Configuration
                        var portConfig = context.Configuration["Port"];
                        if (portConfig != null) 
                        {
                            ApiPort = int.Parse(portConfig);
                        }
                        else
                        {
                            ApiPort = core_api.ConfigurationDefaultVariable.PORT;
                        }

                        CertPath =  context.Configuration.GetSection("Certificate").GetValue<string>("Path");
                        if (CertPath == null || !System.IO.File.Exists(CertPath)) 
                        {
                            throw new Exception("Certificate not exists or not set. Cerificate path: " + CertPath);
                        }
                    } catch (Exception ex) 
                    {
                        if (ex is ArgumentNullException || ex is FormatException || ex is  OverflowException) 
                        {
                            ApiPort = core_api.ConfigurationDefaultVariable.PORT;
                        } 
                        else
                        {
                            throw;
                        }
                    }
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseKestrel(kestrelServerOptions => 
                    {
                        // Listen on any IP
                        kestrelServerOptions.Listen(System.Net.IPAddress.Any, ApiPort, listenOptions => 
                        {
                            // Config ssl pfx
                            listenOptions.UseHttps(CertPath, core_api.ConfigurationDefaultVariable.PASSWORD);
                        });
                    });
                    webBuilder.UseStartup<Startup>();
                });
    }
}
