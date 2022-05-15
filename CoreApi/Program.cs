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
    struct DatabaseAccessConfiguration {
        public string Host      { get; set; }
        public string Port      { get; set; }
        public string User      { get; set; }
        public string DBName    { get; set; }
        public string Password  { get; set; }
        public static DatabaseAccessConfiguration DefaultDatabaseAccessConfiguration { get {
                var Ret         = new DatabaseAccessConfiguration();
                Ret.Host        = "localhost";
                Ret.Port        = "5432";
                Ret.User        = "postgres";
                Ret.DBName      = "db_vip_pro";
                Ret.Password    = "a";
                return Ret;
            }
        }
    }

    public struct EmailClientConfiguration {
        public string Host      { get; set; }
        public string Port      { get; set; }
        public string User      { get; set; }
        public string Password  { get; set; }
        public bool EnableSSL   { get; set; }

        public static EmailClientConfiguration DefaultEmailClientConfiguration { get {
                var Ret         = new EmailClientConfiguration();
                Ret.Host        = string.Empty;
                Ret.Port        = string.Empty;
                Ret.User        = string.Empty;
                Ret.Password    = string.Empty;
                Ret.EnableSSL   = false;
                return Ret;
            }
        }
    }

    public struct SwaggerDocumentConfiguration {
        public bool Enable      { get; set; }
        public string Path      { get; set; }
        public string Username  { get; set; }
        public string Password  { get; set; }

        public static SwaggerDocumentConfiguration DefaultSwaggerDocumentConfiguration { get {
                var Ret = new SwaggerDocumentConfiguration();
#if DEBUG
                Ret.Enable = true;
#else
                Ret.Enable = false;
#endif
                Ret.Username = "admin";
                Ret.Password = "admin";
                Ret.Path = "/api/swagger";
                return Ret;
            }
        }
    }

    public struct ServerConfiguration {
        public int Port                         { get; set; }
        public string HostName                  { get; set; }
        public bool EnableSSL                   { get; set; }
        public bool DisableCORS                 { get; set; }
        public bool ShowSQLCommandInLog         { get; set; }
        public string TempPath                  { get; set; }
        public string CertPath                  { get; set; }
        public string LogFilePath               { get; set; }
        public string LogTemplate               { get; set; }
        public string PasswordCert              { get; set; }
        public string UploadFilePath            { get; set; }
        public string PrefixPathGetUploadFile   { get; set; }

        public static ServerConfiguration DefaultServerConfiguration { get {
                var Ret = new ServerConfiguration();
                Ret.Port                    = 7005;
                Ret.HostName                = "localhost";
                Ret.EnableSSL               = false;
                Ret.DisableCORS             = false;
                Ret.ShowSQLCommandInLog     = false;
                Ret.TempPath                = "./tmp";
                Ret.LogFilePath             = "./tmp/logs/CoreApi-.log";
                Ret.LogTemplate             = "{Timestamp:yyyy-MM-dd HH:mm:ss zzz} [{Level:u3}] ({ThreadId}) {EscapedMessage}{NewLine}{EscapedException}";
                Ret.PasswordCert            = "Ndh90768";
                Ret.UploadFilePath          = "./tmp/upload";
                Ret.PrefixPathGetUploadFile = "/upload/file";
                return Ret;
            }
        }
    }

    public class Program
    {
        public static readonly string APP_NAME                      = "APP-NAME";
        public static readonly string CONFIG_FILE_PATH              = "./appsettings.json";
        private static readonly List<string> __ValidParamsFromArgs  = new List<string>();
        #region Variables
        private static bool __DropDatabase = false;
        private static IHost __Host;
        private static ILogger __Logger;
        private static IConfigurationRoot __Configuration;
        private static ServerConfiguration __ServerConfiguration = ServerConfiguration.DefaultServerConfiguration;
        private static EmailClientConfiguration __EmailClientConfig = EmailClientConfiguration.DefaultEmailClientConfiguration;
        private static DatabaseAccessConfiguration __DBAccessConfig = DatabaseAccessConfiguration.DefaultDatabaseAccessConfiguration;
        private static SwaggerDocumentConfiguration __SwaggerDocumentConfiguration = SwaggerDocumentConfiguration.DefaultSwaggerDocumentConfiguration;
        private static List<string> __ListeningAddress  = new List<string>();
        private static List<string> __AllowMethods      = new List<string>() { "GET", "POST", "DELETE", "PUT" };
        private static List<string> __AllowHeaders      = new List<string>(Common.HEADER_KEYS.GetAllowHeaders());
        #endregion

        #region Public Get Property
        public static List<string> AllowMethods         { get => __AllowMethods; }
        public static List<string> AllowHeaders         { get => __AllowHeaders; }
        public static List<string> ListeningAddress     { get => __ListeningAddress; }
        public static ServerConfiguration ServerConfiguration   { get => __ServerConfiguration; }
        public static EmailClientConfiguration EmailClientConfig { get => __EmailClientConfig; }
        public static SwaggerDocumentConfiguration SwaggerDocumentConfiguration { get => __SwaggerDocumentConfiguration; }
        public static bool DropDatabase { get => __DropDatabase; }
        #endregion
        private static void SetParamsFromConfiguration(in IConfigurationRoot configuration, out List<string> warnings)
        {
            warnings = new List<string>();
            #region Server configuration
            if (configuration.GetSection("Server") != default) {
                // [INFO] Get log file format
                var tmp_LogFilePath = configuration.GetSection("Server").GetValue<string>("LogFilePath");
                if (tmp_LogFilePath == default) {
                    warnings.Add($"Log file path not configured. Use default path: { __ServerConfiguration.LogFilePath }");
                } else {
                    __ServerConfiguration.LogFilePath = tmp_LogFilePath;
                }
                // [INFO] Get port config
                var tmp_PortConfig = configuration.GetSection("Server").GetValue<string>("Port");
                if (tmp_PortConfig != default && CommonValidate.ValidatePort(tmp_PortConfig)) {
                    __ServerConfiguration.Port = int.Parse(tmp_PortConfig);
                } else {
                    if (tmp_PortConfig == default) {
                        warnings.Add($"Port not configured. Use default port: { __ServerConfiguration.Port }");
                    } else {
                        warnings.Add($"Configured port is invalid. Use default port: { __ServerConfiguration.Port }");
                    }
                }
                // [INFO] Get custom upload file path
                var tmp_UploadFilePath = configuration.GetSection("Server").GetValue<string>("UploadFilePath");
                if (tmp_UploadFilePath == default || CommonValidate.ValidateDirectoryPath(tmp_UploadFilePath, true) == default) {
                    warnings.Add($"Upload path is not set or invalid. Use default path: { System.IO.Path.GetFullPath(__ServerConfiguration.UploadFilePath) }");
                } else {
                    __ServerConfiguration.UploadFilePath = tmp_UploadFilePath;
                }
                // [INFO] Get host name from config || default is 'localhost'
                var tmp_HostName = configuration.GetSection("Server").GetValue<string>("HostName");
                if (tmp_HostName == default || !CommonValidate.IsValidDomainName(tmp_HostName)) {
                    warnings.Add($"Invalid host name config. Use default host name: { __ServerConfiguration.HostName }.");
                } else {
                    __ServerConfiguration.HostName = tmp_HostName;
                }
                __ServerConfiguration.HostName = string.Format(
                    "{0}://{1}:{2}",
                    __ServerConfiguration.EnableSSL ? "https" : "http",
                    __ServerConfiguration.HostName,
                    __ServerConfiguration.Port.ToString()
                );
            }
            if (configuration.GetSection("Certificate") != default) {
                // [INFO] Get custom password cert
                var tmp_PasswordCert =  configuration.GetSection("Certificate").GetValue<string>("Password");
                if (tmp_PasswordCert == default || StringDecryptor.Decrypt(tmp_PasswordCert) == default) {
                    warnings.Add($"Password certificate not configured. Use default password: ***");
                } else {
                    __ServerConfiguration.PasswordCert = StringDecryptor.Decrypt(tmp_PasswordCert);
                }
                // [INFO] Get custom cert path
                var tmp_CertPath = configuration.GetSection("Certificate").GetValue<string>("Path");
                if (CommonValidate.ValidateFilePath(tmp_CertPath, false) == default && __ServerConfiguration.EnableSSL) {
                    __ServerConfiguration.EnableSSL = false;
                    warnings.Add($"Certificate not exists or not set. Cerificate path: { ((tmp_CertPath == default) ? default : System.IO.Path.GetFullPath(tmp_CertPath)) }");
                }
            }
            #endregion
            #region Swagger document configuration
            // [INFO] Swagger document
            if (configuration.GetSection("SwaggerDocument") != default) {
                __SwaggerDocumentConfiguration.Enable = configuration.GetSection("SwaggerDocument").GetValue<bool>("Enable");
                __SwaggerDocumentConfiguration.Username = configuration.GetSection("SwaggerDocument").GetValue<string>("Username");
                __SwaggerDocumentConfiguration.Password = configuration.GetSection("SwaggerDocument").GetValue<string>("Password");
                __SwaggerDocumentConfiguration.Path = configuration.GetSection("SwaggerDocument").GetValue<string>("Path");
                __SwaggerDocumentConfiguration.Password = StringDecryptor.Decrypt(__SwaggerDocumentConfiguration.Password == default
                                                            ? string.Empty
                                                            : __SwaggerDocumentConfiguration.Password);
                
                if (__SwaggerDocumentConfiguration.Password == default || __SwaggerDocumentConfiguration.Password == string.Empty) {
                    __SwaggerDocumentConfiguration.Password = Utils.RandomString(15);
                    warnings.Add($"Configured swagger password is invalid. Use random: { __SwaggerDocumentConfiguration.Password }");
                }
                if (__SwaggerDocumentConfiguration.Username == default || __SwaggerDocumentConfiguration.Username == string.Empty) {
                    __SwaggerDocumentConfiguration.Username = Utils.RandomString(15);
                    warnings.Add($"Configured swagger username is invalid. Use random: { __SwaggerDocumentConfiguration.Username }");
                }
                if (__SwaggerDocumentConfiguration.Path == default || __SwaggerDocumentConfiguration.Path == string.Empty
                    || !__SwaggerDocumentConfiguration.Path.StartsWith('/') || __SwaggerDocumentConfiguration.Path.Length < 2
                ) {
                    __SwaggerDocumentConfiguration.Path = Utils.RandomString(5);
                    warnings.Add($"Configured swagger path is invalid. Use random: { __SwaggerDocumentConfiguration.Path }");
                }
            }
            #endregion
            #region Database configuration
            // [INFO] Configure connect string
            if (configuration.GetSection("DatabaseAccess") != default) {
                __DBAccessConfig.Host = configuration.GetSection("DatabaseAccess").GetValue<string>("Host");
                __DBAccessConfig.User = configuration.GetSection("DatabaseAccess").GetValue<string>("Username");
                __DBAccessConfig.Password = configuration.GetSection("DatabaseAccess").GetValue<string>("Password");
                __DBAccessConfig.Port = configuration.GetSection("DatabaseAccess").GetValue<string>("Port");
                __DBAccessConfig.DBName = configuration.GetSection("DatabaseAccess").GetValue<string>("DBName");
                __DBAccessConfig.Password = StringDecryptor.Decrypt(__DBAccessConfig.Password == default ? string.Empty : __DBAccessConfig.Password);

                if (__DBAccessConfig.Port == default || !CommonValidate.ValidatePort(__DBAccessConfig.Port)) {
                    warnings.Add($"Configured database port is invalid or default. Use default port: { IBaseConfigurationDB.Port }");
                    __DBAccessConfig.Port = IBaseConfigurationDB.Port;
                }

                if (__DBAccessConfig.Password == default) {
                    warnings.Add($"Configured database password is invalid or default. Use default password.");
                    __DBAccessConfig.Password = string.Empty;
                }
            } else {
                warnings.Add($"DatabaseAccess configuration not configured. Use default config.");
            }
            BaseConfigurationDB.Configure(
                APP_NAME,
                __DBAccessConfig.Host,
                __DBAccessConfig.User,
                __DBAccessConfig.Password,
                __DBAccessConfig.Port,
                __DBAccessConfig.DBName
            );
            #endregion
            #region Email client configuration
            // [INFO] Configure for email client
            if (configuration.GetSection("Email") != default) {
                __EmailClientConfig.Host = configuration.GetSection("Email").GetValue<string>("Host");
                __EmailClientConfig.User = configuration.GetSection("Email").GetValue<string>("Username");
                __EmailClientConfig.Password = configuration.GetSection("Email").GetValue<string>("Password");
                __EmailClientConfig.Port = configuration.GetSection("Email").GetValue<string>("Port");
                __EmailClientConfig.EnableSSL = configuration.GetSection("Email").GetValue<bool>("EnableSSL");
                __EmailClientConfig.Password = StringDecryptor.Decrypt(__EmailClientConfig.Password == default ? string.Empty : __EmailClientConfig.Password);

                if (__EmailClientConfig.Host == default || __EmailClientConfig.Host == string.Empty) {
                    throw new Exception("Configured email server is invalid or default.");
                }
                if (__EmailClientConfig.Port == default || !CommonValidate.ValidatePort(__EmailClientConfig.Port)) {
                    throw new Exception("Configured email server port is invalid or default.");
                }
                if (__EmailClientConfig.User == default || !CommonValidate.IsEmail(__EmailClientConfig.User)) {
                    throw new Exception("User of email client must be an email.");
                }
                if (__EmailClientConfig.Password == default) {
                    throw new Exception("Invalid password for credential of email client.");
                }
            } else {
                throw new Exception("Missing email client configuration.");
            }
            #endregion
        }
        private static void SetParamsFromArgs(in List<string> args)
        {
            // [INFO] run with param ssl to enable https
            if (args.Contains("ssl")) {
                __ServerConfiguration.EnableSSL = true;
            }
            // [INFO] disable cors policy
            if (args.Contains("disable-cors")) {
                __ServerConfiguration.DisableCORS = true;
            }
            // [INFO] show command query in log
            if (args.Contains("show-sql-command")) {
                __ServerConfiguration.ShowSQLCommandInLog = true;
            }
#if DEBUG
            // [INFO] drop and migrate db when start
            if (args.Contains("drop-db")) {
                __DropDatabase = true;
            }
#else
            __DropDatabase = false;
#endif
        }
        private static string[] GetValidParamsFromArgs(in List<string> args)
        {
            var _args = new List<string>();
            foreach (var it in __ValidParamsFromArgs) {
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
                        kestrelServerOptions.Listen(System.Net.IPAddress.Any, __ServerConfiguration.Port, listenOptions => {
                            if (__ServerConfiguration.EnableSSL && CommonValidate.ValidateFilePath(__ServerConfiguration.CertPath, false) != default) {
                                // Config server using ssl with pfx certificate
                                listenOptions.UseHttps(
                                    CommonValidate.ValidateFilePath(__ServerConfiguration.CertPath, false),
                                    __ServerConfiguration.PasswordCert
                                );
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
            __Logger.Information($"Logs folder: { CommonValidate.ValidateDirectoryPath(System.IO.Path.GetDirectoryName(__ServerConfiguration.LogFilePath)) }");
            __Logger.Information($"Temp folder: { __ServerConfiguration.TempPath }");
            if (Utils.GetIpAddress(out var Ips)) {
                foreach (var ipStr in Ips) {
                    var listeningAddress = string.Format(
                        "{0}://{1}:{2}",
                        __ServerConfiguration.EnableSSL ? "https" : "http",
                        ipStr,
                        __ServerConfiguration.Port.ToString()
                    );
                    __Logger.Information($"Listening on: { listeningAddress }");
                    __ListeningAddress.Add(listeningAddress);
                }
            }
            __Logger.Information($"Host URL: { __ServerConfiguration.HostName }");
        }
        private static void LogEndInformation()
        {
            __Logger.Information("Application is shutting down...");
            __Logger.Information("=================END=================");
        }
        private static ILogger SetDefaultSeriLogger(in IConfigurationRoot configuration)
        {
            Log.Logger = new LoggerConfiguration()
                            .ReadFrom.Configuration(configuration)
                            .Enrich.With(new ExceptionEnricher())
                            .Enrich.With(new MessageEnricher())
                            .Enrich.FromLogContext()
                            .WriteTo.Console(
                                outputTemplate: __ServerConfiguration.LogTemplate
                            )
                            .WriteTo.File(
                                __ServerConfiguration.LogFilePath,
                                outputTemplate: __ServerConfiguration.LogTemplate,
                                rollingInterval: RollingInterval.Day
                            )
                            .CreateLogger();
            return Log.Logger;
        }
        public static void Main(string[] args)
        {
            // [IMPORTANT] Need to run by order
            try {
                if (CommonValidate.ValidateFilePath(CONFIG_FILE_PATH, false) == default) {
                    throw new Exception($"Missing configuration file. Path: { System.IO.Path.GetFullPath(CONFIG_FILE_PATH) }");
                }
                __Configuration = new ConfigurationBuilder()
                                    .AddJsonFile(CONFIG_FILE_PATH)
                                    .AddEnvironmentVariables()
                                    .Build();
                List<string> warningsWhenSetParamsFromConfiguration;
                SetParamsFromArgs(new List<string>(args));
                SetParamsFromConfiguration(__Configuration, out warningsWhenSetParamsFromConfiguration);
                __ServerConfiguration.TempPath = CommonValidate.ValidateDirectoryPath(__ServerConfiguration.TempPath, true);
                __ServerConfiguration.UploadFilePath = CommonValidate.ValidateDirectoryPath(__ServerConfiguration.UploadFilePath, true);
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
