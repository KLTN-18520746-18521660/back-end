using Common;
using CoreApi.Common.Base;
using CoreApi.Services.Background;
using DatabaseAccess.Context;
using DatabaseAccess.Context.Models;
using FluentEmail.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CoreApi.Services
{
    #region email models
    public class BaseEmailModel
    {
        protected string __ModelName;
        public string ModelName { get => __ModelName; }
        public BaseEmailModel()
        {
            __ModelName = "BaseEmailModel";
        }
        virtual public JArray GetAttributes()
        {
            JArray attrs = new JArray();
            foreach (var prop in this.GetType().GetProperties()) {
                if (!prop.Name.Contains("ModelName", StringComparison.OrdinalIgnoreCase)) {
                    attrs.Add(prop.Name);
                }
            }
            return attrs;
        }
    }
    public class UserSignUpEmailModel : BaseEmailModel
    {
        public string UserName          { get; set; }
        public string DisplayName       { get; set; }
        public string ConfirmLink       { get; set; }
        public DateTime DateTimeSend    { get; }
        public UserSignUpEmailModel()
        {
            __ModelName = "UserSignup";
            DateTimeSend = DateTime.UtcNow;
        }
    }
    public class ForgotPasswordEmailModel : BaseEmailModel
    {
        public string UserName              { get; set; }
        public string DisplayName           { get; set; }
        public string ResetPasswordLink     { get; set; }
        public DateTime DateTimeSend        { get; }
        public ForgotPasswordEmailModel()
        {
            __ModelName = "ForgotPassword";
            DateTimeSend = DateTime.UtcNow;
        }
    }
    #endregion
    public class EmailForm
    {
        public string ToEmail { get; set; }
        public string Subject { get; set; }
        public string TraceId { get; set; }
        public BaseEmailModel Model { get; set; }
    }
    public class EmailSender : BaseSingletonService
    {
        private Dictionary<string, string>  __DefaultEmailTemplates;
        private Dictionary<string, string>  __EmailTemplates;
        private SemaphoreSlim               __Gate;
        private int                         __GateLimit;
        private (RequestToSendEmailType EmailType, SUB_CONFIG_KEY SubKey)[] __TemplatePairKeys = new (RequestToSendEmailType, SUB_CONFIG_KEY)[]{
            (RequestToSendEmailType.UserSignup,         SUB_CONFIG_KEY.TEMPLATE_USER_SIGNUP),
            (RequestToSendEmailType.ForgotPassword,     SUB_CONFIG_KEY.TEMPLATE_FORGOT_PASSWORD),
        };
        public EmailSender(IServiceProvider _IServiceProvider) : base(_IServiceProvider)
        {
            __ServiceName           = "EmailSender";
            __GateLimit             = 1;
            __Gate                  = new SemaphoreSlim(__GateLimit);
            __EmailTemplates        = new Dictionary<string, string>();
            __DefaultEmailTemplates = new Dictionary<string, string>();
            LoadDefaultEmailTemplates();
            SetConfigForEmailClient();
        }
        private string LoadLimitSender()
        {
            var __BaseConfig        = (BaseConfig)__ServiceProvider.GetService(typeof(BaseConfig));
            var ErrorMsg            = string.Empty;
            var LimitSenderConfig   = __BaseConfig.GetConfigValue<int>(CONFIG_KEY.EMAIL_CLIENT_CONFIG, SUB_CONFIG_KEY.LIMIT_SENDER);

            if (LimitSenderConfig.Error != string.Empty) {
                WriteLog(LOG_LEVEL.WARNING, string.Empty, "Can not get config 'limit_sender' for email client, use default config.");
            } else {
                if (!ChangeGateLimit(LimitSenderConfig.Value)) {
                    ErrorMsg = $"Can not set 'limit_sender': { LimitSenderConfig.Value } for email client, "
                                + "use default 'limit_sender': { __GateLimit }.";
                    WriteLog(LOG_LEVEL.WARNING, string.Empty, ErrorMsg);
                } else {
                    WriteLog(LOG_LEVEL.INFO, string.Empty, $"Set 'limit_sender' for email client success, value: { LimitSenderConfig.Value }");
                }
            }
            return ErrorMsg;
        }
        private void LoadDefaultEmailTemplates()
        {
            foreach (var It in __TemplatePairKeys) {
                var TemplateData = DEFAULT_BASE_CONFIG.GetConfigValue<string>(CONFIG_KEY.EMAIL_CLIENT_CONFIG, It.SubKey);
                __EmailTemplates[It.EmailType.ToString()]          = TemplateData;
                __DefaultEmailTemplates[It.EmailType.ToString()]   = TemplateData;
            }
        }
        private string[] LoadEmailTemplates()
        {
            var __BaseConfig        = (BaseConfig)__ServiceProvider.GetService(typeof(BaseConfig));
            var Errors              = new List<string>();

            foreach (var It in __TemplatePairKeys) {
                var TemplateConfig = __BaseConfig.GetConfigValue<string>(CONFIG_KEY.EMAIL_CLIENT_CONFIG, It.SubKey);
                if (TemplateConfig.Error != string.Empty) {
                    var ErrMsg = $"Can not get config { DEFAULT_BASE_CONFIG.SubConfigKeyToString(It.SubKey) } for email client, use default config.";
                    WriteLog(LOG_LEVEL.WARNING, string.Empty, ErrMsg);
                    Errors.Add(ErrMsg);
                } else {
                    __EmailTemplates[It.EmailType.ToString()] = TemplateConfig.Value;
                }
            }
            return Errors.ToArray();
        }
        private bool ChangeGateLimit(int Value)
        {
            if (Value == __GateLimit) {
                return true;
            }
            int Diff = Math.Abs(Value - __GateLimit);
            if (Diff == 0 || Value < 1) {
                return false;
            }
            for (int i = 0; i < Diff; i++) {
                if (Value > __GateLimit) {
                    __Gate.Release();
                } else {
                    __Gate.WaitAsync();
                }
            }
            __GateLimit = Value;
            return true;
        }
        private void SetConfigForEmailClient()
        {
            LoadLimitSender();
            LoadEmailTemplates();
        }
        public string[] ReloadEmailConfig()
        {
            var Errors = new List<string>();
            Errors.Add(LoadLimitSender());
            Errors.AddRange(LoadEmailTemplates());
            return Errors.ToArray();
        }

        protected async Task<bool> SendEmail<T>(EmailForm Form) where T : BaseEmailModel
        {
            bool IsReleaseGate = false;
            WriteLog(LOG_LEVEL.INFO, Form.TraceId, $"Received new request email sender, model_name: { Form.Model.ModelName }");
            try {
                if (!__EmailTemplates.ContainsKey(Form.Model.ModelName)) {
                    WriteLog(LOG_LEVEL.ERROR, Form.TraceId, $"Missing template for model { Form.Model.ModelName }");
                    return false;
                }
                var Template = __EmailTemplates.Where(e => e.Key == Form.Model.ModelName).FirstOrDefault().Value;
                if (Utils.BindModelToString<T>(Template, Form.Model as T) == string.Empty) {
                    WriteLog(LOG_LEVEL.WARNING, Form.TraceId, $"Invalid template for model { Form.Model.ModelName }. Using default instead.");
                    Template = __DefaultEmailTemplates.Where(e => e.Key == Form.Model.ModelName).FirstOrDefault().Value;
                }

                await __Gate.WaitAsync();
                var Email = __ServiceProvider.GetService<IFluentEmailFactory>()
                    .Create()
                    .To(Form.ToEmail)
                    .Subject(Form.Subject)
                    .UsingTemplate(Template, (T)Form.Model);
                var Resp = await Email.SendAsync();
                __Gate.Release();
                IsReleaseGate = true;

                if (!Resp.Successful) {
                    WriteLog(LOG_LEVEL.WARNING, Form.TraceId, $"Send email failed.");
                } else {
                    WriteLog(LOG_LEVEL.INFO, Form.TraceId, $"Send email successfully.");
                }
                return Resp.Successful;
            } catch(Exception e) {
                WriteLog(LOG_LEVEL.ERROR, Form.TraceId, "Unhandle Exception", Utils.ParamsToLog("exception_message", e.ToString()));
                if (!IsReleaseGate) {
                    __Gate.Release();
                }
                return false;
            }
        }

        #region Function handler
        public async Task SendEmailUserSignUp(Guid UserId, string TraceId)
        {
            SocialUser User     = default;
            var SendSuccess     = false;
            var RequestState    = string.Empty;
            var PrefixUrl       = string.Empty;
            var HostName        = string.Empty;
            var Subject         = string.Empty;
            var ErrMsg          = string.Empty;

            #region Load config value
            var __BaseConfig    = (BaseConfig)__ServiceProvider.GetService(typeof(BaseConfig));
            (Subject, ErrMsg)   = __BaseConfig.GetConfigValue<string>(CONFIG_KEY.SOCIAL_USER_CONFIRM_CONFIG, SUB_CONFIG_KEY.SUBJECT);
            (HostName, ErrMsg)  = __BaseConfig.GetConfigValue<string>(CONFIG_KEY.SOCIAL_USER_CONFIRM_CONFIG, SUB_CONFIG_KEY.HOST_NAME);
            (PrefixUrl, ErrMsg) = __BaseConfig.GetConfigValue<string>(CONFIG_KEY.SOCIAL_USER_CONFIRM_CONFIG, SUB_CONFIG_KEY.PREFIX_URL);
            #endregion

            using (var scope = __ServiceProvider.CreateScope())
            {
                var __DBContext = scope.ServiceProvider.GetRequiredService<DBContext>();
                User = await __DBContext.SocialUsers
                        .Where(e => e.Id == UserId)
                        .FirstOrDefaultAsync();
                if (User == default) {
                    WriteLog(LOG_LEVEL.WARNING, TraceId, $"Not found user",
                        Utils.ParamsToLog("func_handler", $"{ System.Reflection.MethodBase.GetCurrentMethod().Name }"),
                        Utils.ParamsToLog("user_id", $"{ User.Id }")
                    );
                    return;
                }
                if (User.VerifiedEmail) {
                    WriteLog(LOG_LEVEL.INFO, TraceId, $"User has verified email",
                        Utils.ParamsToLog("func_handler", $"{ System.Reflection.MethodBase.GetCurrentMethod().Name }"),
                        Utils.ParamsToLog("user_id", $"{ User.Id }")
                    );
                    return;
                }
                User.Settings.Remove("confirm_email");
                User.Settings.Add("confirm_email", new JObject(){
                    { "is_sending", true },
                    { "send_date",  DateTime.UtcNow },
                });
                if (await __DBContext.SaveChangesAsync() <= 0) {
                    WriteLog(LOG_LEVEL.ERROR, TraceId, $"Can't save changes before send email",
                        Utils.ParamsToLog("func_handler", $"{ System.Reflection.MethodBase.GetCurrentMethod().Name }"),
                        Utils.ParamsToLog("user_id", $"{ User.Id }")
                    );
                    return;
                }
                #region Send Email
                var UrlConfirm = string.Empty;
                (UrlConfirm, RequestState) = Utils.GenerateUrl(User.Id, HostName, PrefixUrl);
                var Model = new UserSignUpEmailModel() {
                    UserName    = User.UserName,
                    DisplayName = User.DisplayName,
                    ConfirmLink = UrlConfirm,
                };
                SendSuccess = await SendEmail<UserSignUpEmailModel>(new EmailForm() {
                    ToEmail = User.Email,
                    Subject = Subject,
                    TraceId = TraceId,
                    Model   = Model,
                });
                #endregion

                User.Settings.Remove("confirm_email");
                User.Settings.Add("confirm_email", new JObject(){
                    { "is_sending",     false },
                    { "send_success",   SendSuccess },
                    { "send_date",      Model.DateTimeSend },
                    { "confirm_date",   default },
                    { "state",          RequestState },
                });
                if (await __DBContext.SaveChangesAsync() <= 0) {
                    WriteLog(LOG_LEVEL.ERROR, TraceId, $"Can't save changes after send email",
                        Utils.ParamsToLog("func_handler", $"{ System.Reflection.MethodBase.GetCurrentMethod().Name }"),
                        Utils.ParamsToLog("user_id", $"{ User.Id }")
                    );
                    return;
                }
            }
        }
        public async Task SendEmailUserForgotPassword(Guid UserId, string TraceId, bool IsAdminUser = false)
        {
            SocialUser User         = default;
            AdminUser AdminUser     = default;
            var SendSuccess         = false;
            var RequestState        = string.Empty;
            var PrefixUrl           = string.Empty;
            var HostName            = string.Empty;
            var Subject             = string.Empty;
            var ErrMsg              = string.Empty;

            #region Load config value
            var __BaseConfig    = (BaseConfig)__ServiceProvider.GetService(typeof(BaseConfig));
            if (IsAdminUser) {
                (Subject, ErrMsg)   = __BaseConfig.GetConfigValue<string>(CONFIG_KEY.SOCIAL_FORGOT_PASSWORD_CONFIG, SUB_CONFIG_KEY.SUBJECT);
                (HostName, ErrMsg)  = __BaseConfig.GetConfigValue<string>(CONFIG_KEY.SOCIAL_FORGOT_PASSWORD_CONFIG, SUB_CONFIG_KEY.HOST_NAME);
                (PrefixUrl, ErrMsg) = __BaseConfig.GetConfigValue<string>(CONFIG_KEY.SOCIAL_FORGOT_PASSWORD_CONFIG, SUB_CONFIG_KEY.PREFIX_URL);
            } else {
                (Subject, ErrMsg)   = __BaseConfig.GetConfigValue<string>(CONFIG_KEY.SOCIAL_FORGOT_PASSWORD_CONFIG, SUB_CONFIG_KEY.SUBJECT);
                (HostName, ErrMsg)  = __BaseConfig.GetConfigValue<string>(CONFIG_KEY.SOCIAL_FORGOT_PASSWORD_CONFIG, SUB_CONFIG_KEY.HOST_NAME);
                (PrefixUrl, ErrMsg) = __BaseConfig.GetConfigValue<string>(CONFIG_KEY.SOCIAL_FORGOT_PASSWORD_CONFIG, SUB_CONFIG_KEY.PREFIX_URL);
            }
            #endregion

            using (var scope = __ServiceProvider.CreateScope())
            {
                var __DBContext = scope.ServiceProvider.GetRequiredService<DBContext>();
                if (!IsAdminUser) {
                    User = await __DBContext.SocialUsers
                            .Where(e => e.Id == UserId)
                            .FirstOrDefaultAsync();
                    if (User == default) {
                        WriteLog(LOG_LEVEL.WARNING, TraceId, $"Not found user",
                            Utils.ParamsToLog("func_handler", $"{ System.Reflection.MethodBase.GetCurrentMethod().Name }"),
                            Utils.ParamsToLog("is_admin", $"{ IsAdminUser }"),
                            Utils.ParamsToLog("user_id", $"{ User.Id }")
                        );
                        return;
                    }
                    User.Settings.Remove("forgot_password");
                    User.Settings.Add("forgot_password", new JObject(){
                        { "is_sending", true },
                        { "send_date",  DateTime.UtcNow },
                    });
                } else {
                    AdminUser = await __DBContext.AdminUsers
                            .Where(e => e.Id == UserId)
                            .FirstOrDefaultAsync();
                    if (AdminUser == default) {
                        WriteLog(LOG_LEVEL.WARNING, TraceId, $"Not found admin user",
                            Utils.ParamsToLog("func_handler", $"{ System.Reflection.MethodBase.GetCurrentMethod().Name }"),
                            Utils.ParamsToLog("is_admin", $"{ IsAdminUser }"),
                            Utils.ParamsToLog("user_id", $"{ User.Id }")
                        );
                        return;
                    }
                    AdminUser.Settings.Remove("forgot_password");
                    AdminUser.Settings.Add("forgot_password", new JObject(){
                        { "is_sending", true },
                        { "send_date",  DateTime.UtcNow },
                    });
                }
                if (await __DBContext.SaveChangesAsync() < 0) {
                    WriteLog(LOG_LEVEL.ERROR, TraceId, $"Can't save changes before send email",
                        Utils.ParamsToLog("func_handler", $"{ System.Reflection.MethodBase.GetCurrentMethod().Name }"),
                        Utils.ParamsToLog("is_admin", $"{ IsAdminUser }"),
                        Utils.ParamsToLog("user_id", $"{ User.Id }")
                    );
                    return;
                }
                #region Send Email
                var UrlForgot = string.Empty;
                (UrlForgot, RequestState) = Utils.GenerateUrl(IsAdminUser ? AdminUser.Id : User.Id,
                                                                     HostName,
                                                                     PrefixUrl);
                var Model = new ForgotPasswordEmailModel() {
                    UserName            = IsAdminUser ? AdminUser.UserName : User.UserName,
                    DisplayName         = IsAdminUser ? AdminUser.DisplayName : User.DisplayName,
                    ResetPasswordLink   = UrlForgot,
                };
                SendSuccess = await SendEmail<ForgotPasswordEmailModel>(new EmailForm() {
                    ToEmail = User.Email,
                    Subject = Subject,
                    TraceId = TraceId,
                    Model   = Model,
                });
                #endregion

                if (!IsAdminUser) {
                    User.Settings.Remove("forgot_password");
                    User.Settings.Add("forgot_password", new JObject(){
                        { "is_sending",     false },
                        { "send_success",   SendSuccess },
                        { "send_date",      Model.DateTimeSend },
                        { "state",          RequestState },
                    });
                } else {
                    AdminUser.Settings.Remove("forgot_password");
                    AdminUser.Settings.Add("forgot_password", new JObject(){
                        { "is_sending",     false },
                        { "send_success",   SendSuccess },
                        { "send_date",      Model.DateTimeSend },
                        { "state",          RequestState },
                    });
                }
                if (await __DBContext.SaveChangesAsync() <= 0) {
                    WriteLog(LOG_LEVEL.ERROR, TraceId, $"Can't save changes after send email",
                        Utils.ParamsToLog("func_handler", $"{ System.Reflection.MethodBase.GetCurrentMethod().Name }"),
                        Utils.ParamsToLog("is_admin", $"{ IsAdminUser }"),
                        Utils.ParamsToLog("user_id", $"{ User.Id }")
                    );
                    return;
                }
            }
        }
        #endregion
    }
}
