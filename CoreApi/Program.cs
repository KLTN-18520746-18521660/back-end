using Common;
using Common.Logger;
using DatabaseAccess;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Collections.Generic;

namespace CoreApi
{
    static class ConfigurationDefaultVariable
    {
        public static readonly string CONFIG_FILE_PATH = "./appsettings.json";
        public static readonly int PORT = 7005;
        // [INFO] Password default of certificate
        public static readonly string PASSWORD_CERTIFICATE = "Ndh90768";
        public static readonly string LOG_FILE_FORMAT = "./tmp/logs/CoreApi.log";
        public static readonly string LOG_TEMPLATE = "{Timestamp:yyyy-MM-dd HH:mm:ss zzz} [{Level:u3}] ({ThreadId}) {EscapedMessage}{NewLine}{EscapedException}";
        public static readonly string TMP_FOLDER = "./tmp";
    }

    struct DatabaseAccessConfiguration {
        public string Host { get; set; }
        public string Port { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string DBName { get; set; }
    }

    public struct EmailClientConfiguration {
        public string Host { get; set; }
        public string Port { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public bool EnableSSL { get; set; }
    }

    public struct SwaggerDocumentConfiguration {
        public bool Enable { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Path { get; set; }
    }

    public class Program
    {
        #region Variables
        private static ILogger __Logger;
        private static IConfigurationRoot __Configuration;
        private static IHost __Host;
        private static int _Port;
        private static bool _EnableSSL = false;
        private static bool _DisableCORS = false;
        private static bool _ShowSQLCommandInLog = false;
        private static string _TmpPath = ConfigurationDefaultVariable.TMP_FOLDER;
        private static string _CertPath;
        private static string _LogFilePath;
        private static string _PasswordCert;
        private static DatabaseAccessConfiguration _DBAccessConfig;
        private static EmailClientConfiguration _EmailClientConfig;
        private static SwaggerDocumentConfiguration _SwaggerDocumentConfiguration;
        private static readonly List<string> _ValidParamsFromArgs = new List<string>();
        private static string _HostName;
        private static List<string> _ListeningAddress = new List<string>();
        private static List<string> _AllowMethods = new List<string>() { "GET", "POST", "DELETE", "PUT" };
        private static List<string> _AllowHeaders = new List<string>() { "session_token" };
        #endregion
        #region Property
        public static string HostName { get => _HostName; }
        public static bool EnableSSL { get => _EnableSSL; }
        public static bool DisableCORS { get => _DisableCORS; }
        public static bool ShowSQLCommandInLog { get => _ShowSQLCommandInLog; }
        public static List<string> ListeningAddress { get => _ListeningAddress; }
        public static List<string> AllowMethods { get => _AllowMethods; }
        public static List<string> AllowHeaders { get => _AllowHeaders; }
        public static EmailClientConfiguration EmailClientConfig { get => _EmailClientConfig;  }
        public static SwaggerDocumentConfiguration SwaggerDocumentConfiguration { get => _SwaggerDocumentConfiguration;  }
        #endregion
        private static void SetParamsFromConfiguration(in IConfigurationRoot configuration, out List<string> warnings)
        {
            warnings = new List<string>();
            // [INFO] Get log file format
            _LogFilePath = configuration.GetValue<string>("LogFilePath");
            if (_LogFilePath == default) {
                _LogFilePath = ConfigurationDefaultVariable.LOG_FILE_FORMAT;
                warnings.Add($"Log file path not configured. Use default path: { _LogFilePath }");
            }
            // [INFO] Get port config
            var portConfig = configuration["Port"];
            if (portConfig != default && CommonValidate.ValidatePort(portConfig)) {
                _Port = int.Parse(portConfig);
            } else {
                _Port = ConfigurationDefaultVariable.PORT;
                if (portConfig == default) {
                    warnings.Add($"Port not configured. Use default port: { _Port }");
                } else {
                    warnings.Add($"Configured port is invalid. Use default port: { _Port }");
                }
            }
            // [INFO] Get custom passeord cert
            _PasswordCert =  configuration.GetSection("Certificate").GetValue<string>("Password");
            if (_PasswordCert == default || StringDecryptor.Decrypt(_PasswordCert) == default) {
                _PasswordCert = ConfigurationDefaultVariable.PASSWORD_CERTIFICATE;
                warnings.Add($"Password certificate not configured. Use default password: ***");
            } else {
                _PasswordCert = StringDecryptor.Decrypt(_PasswordCert);
            }
            // [INFO] Get custom cert path
            _CertPath = configuration.GetSection("Certificate").GetValue<string>("Path");
            if (CommonValidate.ValidateFilePath(_CertPath, false) == default && _EnableSSL) {
                _EnableSSL = false;
                warnings.Add($"Certificate not exists or not set. Cerificate path: { ((_CertPath == default) ? default : System.IO.Path.GetFullPath(_CertPath)) }");
            }
            // [INFO] Swagger document
            if (configuration.GetSection("SwaggerDocument") != default) {
                _SwaggerDocumentConfiguration.Enable = configuration.GetSection("SwaggerDocument").GetValue<bool>("Enable");
                _SwaggerDocumentConfiguration.Username = configuration.GetSection("SwaggerDocument").GetValue<string>("Username");
                _SwaggerDocumentConfiguration.Password = configuration.GetSection("SwaggerDocument").GetValue<string>("Password");
                _SwaggerDocumentConfiguration.Path = configuration.GetSection("SwaggerDocument").GetValue<string>("Path");
                _SwaggerDocumentConfiguration.Password = StringDecryptor.Decrypt(_SwaggerDocumentConfiguration.Password == default
                                                            ? string.Empty
                                                            : _SwaggerDocumentConfiguration.Password);
                
                if (_SwaggerDocumentConfiguration.Password == default || _SwaggerDocumentConfiguration.Password == string.Empty) {
                    _SwaggerDocumentConfiguration.Password = Utils.RandomString(15);
                    warnings.Add($"Configured swagger password is invalid. Use random: { _SwaggerDocumentConfiguration.Password }");
                }
                if (_SwaggerDocumentConfiguration.Username == default || _SwaggerDocumentConfiguration.Username == string.Empty) {
                    _SwaggerDocumentConfiguration.Username = Utils.RandomString(15);
                    warnings.Add($"Configured swagger username is invalid. Use random: { _SwaggerDocumentConfiguration.Username }");
                }
                if (_SwaggerDocumentConfiguration.Path == default || _SwaggerDocumentConfiguration.Path == string.Empty
                    || !_SwaggerDocumentConfiguration.Path.StartsWith('/') || _SwaggerDocumentConfiguration.Path.Length < 2
                ) {
                    _SwaggerDocumentConfiguration.Path = Utils.RandomString(5);
                    warnings.Add($"Configured swagger path is invalid. Use random: { _SwaggerDocumentConfiguration.Path }");
                }
            }
            // [INFO] Configure connect string
            if (configuration.GetSection("DatabaseAccess") != default) {
                _DBAccessConfig.Host = configuration.GetSection("DatabaseAccess").GetValue<string>("Host");
                _DBAccessConfig.User = configuration.GetSection("DatabaseAccess").GetValue<string>("Username");
                _DBAccessConfig.Password = configuration.GetSection("DatabaseAccess").GetValue<string>("Password");
                _DBAccessConfig.Port = configuration.GetSection("DatabaseAccess").GetValue<string>("Port");
                _DBAccessConfig.DBName = configuration.GetSection("DatabaseAccess").GetValue<string>("DBName");
                _DBAccessConfig.Password = StringDecryptor.Decrypt(_DBAccessConfig.Password == default ? string.Empty : _DBAccessConfig.Password);

                if (_DBAccessConfig.Port == default || !CommonValidate.ValidatePort(_DBAccessConfig.Port)) {
                    warnings.Add($"Configured database port is invalid or default. Use default port: { IBaseConfigurationDB.Port }");
                    _DBAccessConfig.Port = IBaseConfigurationDB.Port;
                }

                if (_DBAccessConfig.Password == default) {
                    warnings.Add($"Configured database password is invalid or default. Use default password.");
                    _DBAccessConfig.Password = string.Empty;
                }

                BaseConfigurationDB.Configure(_DBAccessConfig.Host, _DBAccessConfig.User, _DBAccessConfig.Password, _DBAccessConfig.Port, _DBAccessConfig.DBName);
            } else {
                warnings.Add($"DatabaseAccess configuration not configured. Use default config.");
                BaseConfigurationDB.Configure(); // Use default value
            }
            // [INFO] Configure for email client
            if (configuration.GetSection("Email") != default) {
                _EmailClientConfig.Host = configuration.GetSection("Email").GetValue<string>("Host");
                _EmailClientConfig.User = configuration.GetSection("Email").GetValue<string>("Username");
                _EmailClientConfig.Password = configuration.GetSection("Email").GetValue<string>("Password");
                _EmailClientConfig.Port = configuration.GetSection("Email").GetValue<string>("Port");
                _EmailClientConfig.EnableSSL = configuration.GetSection("Email").GetValue<bool>("EnableSSL");
                _EmailClientConfig.Password = StringDecryptor.Decrypt(_EmailClientConfig.Password == default ? string.Empty : _EmailClientConfig.Password);

                if (_EmailClientConfig.Host == default || _EmailClientConfig.Host == string.Empty) {
                    throw new Exception("Configured email server is invalid or default.");
                }
                if (_EmailClientConfig.Port == default || !CommonValidate.ValidatePort(_EmailClientConfig.Port)) {
                    throw new Exception("Configured email server port is invalid or default.");
                }
                if (_EmailClientConfig.User == default || !CommonValidate.IsEmail(_EmailClientConfig.User)) {
                    throw new Exception("User of email client must be an email.");
                }
                if (_EmailClientConfig.Password == default) {
                    throw new Exception("Invalid password for credential of email client.");
                }
            } else {
                throw new Exception("Missing email client configuration.");
            }
            // [INFO] Get host name from config || default is 'localhost'
            _HostName = configuration.GetValue<string>("HostName");
            if (_HostName == default || !CommonValidate.IsValidDomainName(_HostName)) {
                _HostName = "localhost";
                warnings.Add($"Invalid host name config. Use default host name: { _HostName }.");
            }
            _HostName = string.Format(
                "{0}://{1}:{2}",
                _EnableSSL ? "https" : "http",
                _HostName,
                _Port.ToString()
            );
        }
        private static void SetParamsFromArgs(in List<string> args)
        {
            // [INFO] run with param ssl to enable https
            if (args.Contains("ssl")) {
                _EnableSSL = true;
            }
            // [INFO] disable cors policy
            if (args.Contains("disable-cors")) {
                _DisableCORS = true;
            }
            // [INFO] show command query in log
            if (args.Contains("show-sql-command")) {
                _ShowSQLCommandInLog = true;
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
#if DEBUG
                .UseEnvironment(Environments.Development)
#else
                .UseEnvironment(Environments.Production)
#endif
                .UseSerilog()
                .UseConsoleLifetime()
                .ConfigureServices(s => s.AddSingleton<IConfigurationRoot>(__Configuration))
                .ConfigureWebHostDefaults(webBuilder => {
                    webBuilder.UseKestrel(kestrelServerOptions => {
                        // [INFO] Listen on any IP
                        kestrelServerOptions.Listen(System.Net.IPAddress.Any, _Port, listenOptions => {
                            if (_EnableSSL && CommonValidate.ValidateFilePath(_CertPath, false) != default) {
                                // Config server using ssl with pfx certificate
                                listenOptions.UseHttps(CommonValidate.ValidateFilePath(_CertPath, false), _PasswordCert);
                            }
                        });
                        kestrelServerOptions.AddServerHeader = false;
                    });
                    webBuilder.UseUrls();
                    webBuilder.UseStartup<Startup>();
                });
        private static void LogStartInformation()
        {
            __Logger.Information("=================START=================");
#if DEBUG
            __Logger.Warning("The application is compiled in debug mode.");
#endif
            __Logger.Information($"Logs folder: { CommonValidate.ValidateDirectoryPath(System.IO.Path.GetDirectoryName(_LogFilePath)) }");
            __Logger.Information($"Temp folder: { _TmpPath }");
            if (Utils.GetIpAddress(out var Ips)) {
                foreach (var ipStr in Ips) {
                    var listeningAddress = string.Format(
                        "{0}://{1}:{2}",
                        _EnableSSL ? "https" : "http",
                        ipStr,
                        _Port.ToString()
                    );
                    __Logger.Information($"Listening on: { listeningAddress }");
                    _ListeningAddress.Add(listeningAddress);
                }
            }
            __Logger.Information($"Host URL: { _HostName }");
        }
        private static void LogEndInformation()
        {
            __Logger.Information("Application is shutting down...");
            __Logger.Information("=================END=================");
        }
        private static ILogger SetDefaultSeriLogger(in IConfigurationRoot configuration)
        {
            if (_LogFilePath == default) {
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
            // [IMPORTANT] Need to run by order
            try {
                if (CommonValidate.ValidateFilePath(ConfigurationDefaultVariable.CONFIG_FILE_PATH, false) == default) {
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
            } catch (Exception ex) {
                if (__Logger != default) {
                    __Logger.Error(ex.ToString());
                } else {
                    __Logger = SetDefaultSeriLogger(new ConfigurationBuilder().Build());
                    LogStartInformation();
                    __Logger.Error(ex.ToString());
                }
            }
            LogEndInformation();
            Log.CloseAndFlush();
        }
    }
}
