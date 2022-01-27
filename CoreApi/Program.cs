using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using Serilog;
using Common.Validate;
using Common.Logger;

namespace CoreApi
{
    public static class ConfigurationDefaultVariable
    {
        public static readonly string CONFIG_FILE_PATH = "./appsettings.json";
        public static readonly int PORT = 7005;
        // Password default of certificate
        public static readonly string PASSWORD_CERTIFICATE = "Ndh90768";
        public static readonly string LOG_FILE_FORMAT = "./tmp/logs/CoreApi.log";
        public static readonly string LOG_TEMPLATE = "{Timestamp:yyyy-MM-dd HH:mm:ss zzz} [{Level:u3}] {EscapedMessage}{NewLine}{EscapedException}";
        public static readonly string TMP_FOLDER = "./tmp";
    }
    public class Program
    {
        private static ILogger __Logger;
        private static IConfigurationRoot __Configuration;
        private static IHost __Host;
        private static int _Port;
        private static bool _EnableSSL;
        private static string _TmpPath = ConfigurationDefaultVariable.TMP_FOLDER;
        private static string _CertPath;
        private static string _LogFilePath;
        private static string _PasswordCert;
        private static readonly List<string> _ValidParamsFromArgs = new List<string>();
        private static void SetParamsFromConfiguration(in IConfigurationRoot configuration, out List<string> warnings)
        {
            warnings = new List<string>();
            // [INFO] Get log file format
            _LogFilePath = configuration.GetValue<string>("LogFilePath");
            if (_LogFilePath == null) {
                _LogFilePath = ConfigurationDefaultVariable.LOG_FILE_FORMAT;
                warnings.Add($"Log file path not configured. Use default path: { _LogFilePath }");
            }
            // [INFO] Get port config
            var portConfig = configuration["Port"];
            if (portConfig != null && CommonValidate.ValidatePort(portConfig)) {
                _Port = int.Parse(portConfig);
            } else {
                _Port = ConfigurationDefaultVariable.PORT;
                if (portConfig == null) {
                    warnings.Add($"Port not configured. Use default port: { _Port }");
                } else {
                    warnings.Add($"Configured port isn't valid. Use default port: { _Port }");
                }
            }
            // [INFO] Get custom passeord cert
            _PasswordCert =  configuration.GetSection("Certificate").GetValue<string>("Password");
            if (_PasswordCert == null) {
                _PasswordCert = ConfigurationDefaultVariable.PASSWORD_CERTIFICATE;
                warnings.Add($"Password certificate not configured. Use default password: ***");
            }
            // [INFO] Get custom cert path
            _CertPath = configuration.GetSection("Certificate").GetValue<string>("Path");
            if (CommonValidate.ValidateFilePath(_CertPath, false) == null && _EnableSSL) {
                _EnableSSL = false;
                warnings.Add($"Certificate not exists or not set. Cerificate path: { ((_CertPath == null) ? null : System.IO.Path.GetFullPath(_CertPath)) }");
            }
        }
        private static void SetParamsFromArgs(in List<string> args)
        {
            // [INFO] run with param ssl to enable https
            if (args.Contains("ssl")) {
                _EnableSSL = true;
            }
        }
        private static string[] GetValidParamsFromArgs(in List<string> args)
        {
            var _args = new List<string>();
            foreach (var it in _ValidParamsFromArgs) {
                if (args.Contains(it)) {
                    _args.Add(it);
                }
            }
            return args.ToArray();
        }
        private static IHostBuilder CreateHostBuilder(in string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) => {
                    // services.Add()
                })
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder => {
                    webBuilder.UseKestrel(kestrelServerOptions => {
                        // Listen on any IP
                        kestrelServerOptions.Listen(System.Net.IPAddress.Any, _Port, listenOptions => {
                            if (_EnableSSL && CommonValidate.ValidateFilePath(_CertPath, false) != null) {
                                // Config ssl pfx
                                listenOptions.UseHttps(CommonValidate.ValidateFilePath(_CertPath, false), _PasswordCert);
                            }
                        });
                    });
                    webBuilder.UseUrls();
                    webBuilder.UseStartup<Startup>();
                });
        private static void LogStartInformation()
        {
            __Logger.Information("=================START=================");
            __Logger.Information($"Logs folder: { CommonValidate.ValidateDirectoryPath(System.IO.Path.GetDirectoryName(_LogFilePath)) }");
            __Logger.Information($"Temp folder: { _TmpPath }");
            __Logger.Information( string.Format(
                "Listening on: {0}://{1}:{2}",
                _EnableSSL ? "https" : "http",
                System.Net.IPAddress.Any.ToString(),
                _Port.ToString()
            ));
        }
        private static void LogEndInformation()
        {
            __Logger.Information("Application is shutting down...");
            __Logger.Information("=================END=================");
        }
        private static ILogger SetDefaultSeriLogger(in IConfigurationRoot configuration)
        {
            if (_LogFilePath == null) {
                _LogFilePath = ConfigurationDefaultVariable.LOG_FILE_FORMAT;
            }
            Log.Logger = new LoggerConfiguration()
                            .ReadFrom.Configuration(configuration)
                            .Enrich.With(new ExceptionEnricher())
                            .Enrich.With(new MessageEnricher())
                            .Enrich.FromLogContext()
                            .WriteTo.Console(
                                outputTemplate: ConfigurationDefaultVariable.LOG_TEMPLATE
                            )
                            .WriteTo.File(
                                _LogFilePath,
                                outputTemplate: ConfigurationDefaultVariable.LOG_TEMPLATE,
                                rollingInterval: RollingInterval.Day
                            )
                            .CreateLogger();
            return Log.Logger;
        }
        public static void Main(string[] args)
        {
            // Need to run by order
            try {
                if (CommonValidate.ValidateFilePath(ConfigurationDefaultVariable.CONFIG_FILE_PATH, false) == null) {
                    throw new Exception($"Missing configuration file. Path: { System.IO.Path.GetFullPath(ConfigurationDefaultVariable.CONFIG_FILE_PATH) }");
                }
                __Configuration = new ConfigurationBuilder()
                                    .AddJsonFile(ConfigurationDefaultVariable.CONFIG_FILE_PATH)
                                    .AddEnvironmentVariables()
                                    .Build();
                List<string> warningsWhenSetParamsFromConfiguration;
                SetParamsFromArgs(new List<string>(args));
                SetParamsFromConfiguration(__Configuration, out warningsWhenSetParamsFromConfiguration);
                _TmpPath = CommonValidate.ValidateDirectoryPath(_TmpPath);
                __Logger = SetDefaultSeriLogger(__Configuration);
                LogStartInformation();
                warningsWhenSetParamsFromConfiguration.ForEach(message => {
                    __Logger.Warning(message);
                });

                string[] _args = GetValidParamsFromArgs(new List<string>(args));
                __Host = CreateHostBuilder(_args).Build();
                __Host.Run();
            } 
            catch (Exception ex) {
                if (__Logger != null) {
                    __Logger.Error(ex.ToString());
                } else {
                    __Logger = SetDefaultSeriLogger(new ConfigurationBuilder().Build());
                    LogStartInformation();
                    __Logger.Error(ex.ToString());
                }
            }
            LogEndInformation();
        }
    }
}
