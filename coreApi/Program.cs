using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace coreApi
{
    public static class ConfigurationDefaultVariable
    {
        public static readonly int PORT = 7005;
        // Password default of certificate
        public static readonly string PASSWORD_CERTIFICATE = "Ndh90768";
        public static readonly string LOG_FILE_FORMAT = "./logs/coreApi-{Date}.log";
        public static readonly string TMP_FOLDER = "./tmp";
    }
    public class Program
    {
        private static int apiPort;
        private static bool enableSSL;
        private static ILogger _logger;
        private static ILoggerFactory _loggerFactory; // config in CreateHostBuilder()
        private static string tmpPath = ConfigurationDefaultVariable.TMP_FOLDER;
        private static string certPath;
        private static string logFileFormat;
        private static List<string> validParamsFromArgs = new List<string>();
        ////////////// public variable
        public static int Port { get => apiPort; }
        public static ILoggerFactory BaseLoggerFactory { get => _loggerFactory; }

        /////////////
        public static void SetParamsFromArgs(List<string> args) 
        {
            if (args.Contains("ssl")) {
                enableSSL = true;
            }
        }
        public static string[] GetValidParamsFromArgs(List<string> args)
        {
            var _args = new List<string>();
            foreach (var it in validParamsFromArgs) {
                if (args.Contains(it)) {
                    _args.Add(it);
                }
            }
            return args.ToArray();
        }
        public static string ValidateFilePath(in string filePath)
        {
            if (!System.IO.File.Exists(filePath)) {
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(filePath));
                var fileCreated = System.IO.File.Create(filePath);
                fileCreated.Close();
            }
            var file = System.IO.File.Open(filePath, System.IO.FileMode.Append);
            file.Flush();
            file.Close();
            return System.IO.Path.GetFullPath(filePath);
        }
        public static string ValidateDirectoryPath(in string dirPath)
        {
            if (!System.IO.Directory.Exists(dirPath)) {
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(dirPath));
            }
            return System.IO.Path.GetFullPath(dirPath);
        }
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging((context, loggerBuilder) => {
                    logFileFormat = context.Configuration.GetValue<string>("LogFileFormat");
                    if (logFileFormat == null) {
                        logFileFormat = ConfigurationDefaultVariable.LOG_FILE_FORMAT;
                    }
                    loggerBuilder.ClearProviders();
                    loggerBuilder.AddConfiguration(context.Configuration.GetSection("Logging"));
                    loggerBuilder.AddConsole();
                    loggerBuilder.AddFile(logFileFormat);
                    
                    // Copy logFactory for create new service with log
                    _loggerFactory = LoggerFactory.Create(_loggerBuilder => {
                        _loggerBuilder.ClearProviders();
                        _loggerBuilder.AddConfiguration(context.Configuration.GetSection("Logging"));
                        _loggerBuilder.AddConsole();
                        _loggerBuilder.AddFile(logFileFormat);
                    });
                })
                .ConfigureServices((context, service) => {
                    try {
                        // [INFO] Config custom port from Configuration
                        var portConfig = context.Configuration["Port"];
                        if (portConfig != null) {
                            apiPort = int.Parse(portConfig);
                        }
                        else {
                            apiPort = ConfigurationDefaultVariable.PORT;
                        }

                        certPath =  context.Configuration.GetSection("Certificate").GetValue<string>("Path");
                        if ((certPath == null || !System.IO.File.Exists(certPath)) && enableSSL) {
                            throw new Exception($"Certificate not exists or not set. Cerificate path: {certPath}");
                        }
                    } catch (Exception ex) {
                        if (ex is ArgumentNullException || ex is FormatException || ex is  OverflowException) {
                            apiPort = ConfigurationDefaultVariable.PORT;
                        } 
                        else {
                            throw;
                        }
                    }
                })
                .ConfigureWebHostDefaults(webBuilder => {
                    webBuilder.UseKestrel(kestrelServerOptions => {
                        // Listen on any IP
                        kestrelServerOptions.Listen(System.Net.IPAddress.Any, apiPort, listenOptions => {
                            if (enableSSL) {
                                // Config ssl pfx
                                listenOptions.UseHttps(certPath, ConfigurationDefaultVariable.PASSWORD_CERTIFICATE);
                            }
                        });
                    });
                    webBuilder.UseUrls();
                    webBuilder.UseStartup<Startup>();
                });
        private static void LogStartInformation()
        {
            _logger.LogInformation(0, "=================START=================");
            _logger.LogInformation(0, "Logs folder: {logPath}", ValidateDirectoryPath(System.IO.Path.GetDirectoryName(logFileFormat)));
            _logger.LogInformation(0, "Temp folder: {tmpPath}", tmpPath);
        }
        private static void LogEndInformation()
        {
             _logger.LogInformation(0, "=================END=================");
        }
        public static void Main(string[] args)
        {
            // Need to run by order
            SetParamsFromArgs(new List<string>(args));
            tmpPath = ValidateDirectoryPath(tmpPath);

            var _args = GetValidParamsFromArgs(new List<string>(args));
            IHost host;
            try {
                host = CreateHostBuilder(_args).Build();
                _logger = _loggerFactory.CreateLogger<Program>();
                LogStartInformation();
                host.Run();
            } 
            catch (Exception e) {
                _logger = _loggerFactory.CreateLogger<Program>();
                LogStartInformation();
                Common.Logger.LogException(0, _logger, e.ToString());
            }

            LogEndInformation();
            _loggerFactory.Dispose();
        }
    }
}
