using CoreApi.Common;
using DatabaseAccess.Context;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DatabaseAccess.Context.Models;
using FluentEmail.Core;
using Newtonsoft.Json.Linq;
using Common;
using Newtonsoft.Json;
using Microsoft.Extensions.DependencyInjection;

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
            foreach(var prop in this.GetType().GetProperties()) {
                if (!prop.Name.Contains("ModelName", StringComparison.OrdinalIgnoreCase)) {
                    attrs.Add(prop.Name);
                }
            }
            return attrs;
        }
    }
    public class UserSignUpEmailModel : BaseEmailModel
    {
        public string UserName { get; set; }
        public string ConfirmLink { get; set; }
        public DateTime DateTimeSend { get; }
        public UserSignUpEmailModel()
        {
            __ModelName = "UserSignup";
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
    public class EmailSender : BaseService
    {
        private Dictionary<string, string> __EmailTemplates = new Dictionary<string, string>()
        {
            { "UserSignup", @"<p>Dear @Model.UserName,</p>
                            <p>Confirm link here: <a href='@Model.ConfirmLink'>@Model.ConfirmLink</a><br>
                            Send datetime: @Model.DateTimeSend</p>
                            <p>Thanks for your register.</p>" }
        };
        private SemaphoreSlim __Gate;
        private int __GateLimit;
        private IFluentEmail __Email;
        public EmailSender(DBContext _DBContext, IServiceProvider _IServiceProvider, IFluentEmail _Email) : base(_DBContext, _IServiceProvider)
        {
            __Email = _Email;
            __ServiceName = "EmailSender";
            __GateLimit = 1;
            __Gate = new SemaphoreSlim(__GateLimit);
            SetConfigForEmailClient();
        }

        private void SetConfigForEmailClient()
        {
            var __BaseConfig = (BaseConfig)__ServiceProvider.GetService(typeof(BaseConfig));

            #region limit_sender
            var limitSenderRs = __BaseConfig.GetConfigValue<int>(CONFIG_KEY.EMAIL_CLIENT_CONFIG, SUB_CONFIG_KEY.EMAIL_LIMIT_SENDER);

            if (limitSenderRs.Error != string.Empty) {
                LogWarning("Can not get config 'limit_sender' for email client, use default config.");
            } else {
                if (!ChangeGateLimit(limitSenderRs.Value)) {
                    LogWarning($"Can not set 'limit_sender': { limitSenderRs.Value } for email client, use default 'limit_sender': { __GateLimit }.");
                } else {
                    LogInformation($"Set 'limit_sender' for email client success, value: { limitSenderRs.Value }");
                }
            }
            #endregion

            #region email_templates
            var templateUserSignupRs = __BaseConfig.GetConfigValue<string>(CONFIG_KEY.EMAIL_CLIENT_CONFIG, SUB_CONFIG_KEY.EMAIL_TEMPLATE_USER_SIGNUP);

            if (templateUserSignupRs.Error != string.Empty) {
                LogWarning("Can not get config 'template_user_signup' for email client, use default config.");
            } else {
                __EmailTemplates.Remove("UserSignup");
                __EmailTemplates.Add("UserSignup", templateUserSignupRs.Value);
            }
            #endregion
        }

        private bool ChangeGateLimit(int value)
        {
            int diff = Math.Abs(value - __GateLimit);
            if (diff == 0 || value < 1) {
                return false;
            }
            for (int i = 0; i < diff; i++) {
                if (value > __GateLimit) {
                    __Gate.Release();
                } else {
                    __Gate.WaitAsync();
                }
            }
            __GateLimit = value;
            return true;
        }

        public async Task<bool> ReloadEmailConfig()
        {
            var __BaseConfig = (BaseConfig)__ServiceProvider.GetService(typeof(BaseConfig));
            _ = await __BaseConfig.ReLoadConfig();
            #region limit_sender
            var (Value, Error) = __BaseConfig.GetConfigValue<int>(CONFIG_KEY.EMAIL_CLIENT_CONFIG, SUB_CONFIG_KEY.EMAIL_LIMIT_SENDER);

            if (Error != string.Empty) {
                LogWarning("Can not get config for email client, use default config.");
                return false;
            }

            if (!ChangeGateLimit(Value)) {
                LogWarning($"Can not set 'limit_sender': { Value } for email client, use default 'limit_sender': { __GateLimit }.");
            } else {
                LogInformation($"Set 'limit_sender' for email client success, value: { Value }");
            }
            #endregion

            #region email_templates
            var templateUserSignupRs = __BaseConfig.GetConfigValue<string>(CONFIG_KEY.EMAIL_CLIENT_CONFIG, SUB_CONFIG_KEY.EMAIL_TEMPLATE_USER_SIGNUP);

            if (templateUserSignupRs.Error != string.Empty) {
                LogWarning("Can not get config 'template_user_signup' for email client, use default config.");
            } else {
                __EmailTemplates.Remove("UserSignup");
                __EmailTemplates.Add("UserSignup", templateUserSignupRs.Value);
            }
            #endregion

            return true;
        }

        protected async Task<bool> SendEmail<T>(EmailForm form) where T : BaseEmailModel
        {
            bool isReleaseGate = false;
            LogInformation($"TraceId: { form.TraceId }, Received new request email sender, modelname: { form.Model.ModelName }");
            try {
                if (!__EmailTemplates.ContainsKey(form.Model.ModelName)) {
                    LogWarning($"TraceId: { form.TraceId }, Invalid template for model, modelname: { form.Model.ModelName }");
                    return false;
                }
                var template = __EmailTemplates.Where(e => e.Key == form.Model.ModelName).FirstOrDefault().Value;

                await __Gate.WaitAsync();
                
                var email = __Email
                    .To(form.ToEmail)
                    .Subject(form.Subject)
                    .UsingTemplate(template, (T)form.Model);
                var resp = await email.SendAsync();
                __Gate.Release();
                isReleaseGate = true;

                if (!resp.Successful) {
                    LogWarning($"TraceId: { form.TraceId }, Send email failed.");
                } else {
                    LogInformation($"TraceId: { form.TraceId }, Send email successfully.");
                }

                return resp.Successful;
            } catch(Exception e) {
                LogError($"TraceId: { form.TraceId }, Unhandle Exception, { e }");
                if (!isReleaseGate) {
                    __Gate.Release();
                }
                return false;
            }
        }

        #region Function handler
        public async Task SendEmailUserSignUp(Guid UserId, string TraceId)
        {
            SocialUser user = default;
            var sendSuccess = false;
            var requestState = string.Empty;
            var hostName = string.Empty;
            var prefixUrl = string.Empty;
            var errMsg = string.Empty;

            #region Load config value
            var __BaseConfig = (BaseConfig)__ServiceProvider.GetService(typeof(BaseConfig));
            (hostName, errMsg) = __BaseConfig.GetConfigValue<string>(CONFIG_KEY.SOCIAL_USER_CONFIRM_CONFIG, SUB_CONFIG_KEY.HOST_NAME);
            (prefixUrl, errMsg) = __BaseConfig.GetConfigValue<string>(CONFIG_KEY.SOCIAL_USER_CONFIRM_CONFIG, SUB_CONFIG_KEY.PREFIX_URL);
            #endregion

            using (var scope = __ServiceProvider.CreateScope())
            {
                var __SocialUserManagement = scope.ServiceProvider.GetRequiredService<SocialUserManagement>();
                ErrorCodes error = ErrorCodes.NO_ERROR;
                (user, error) = await __SocialUserManagement.FindUserById(UserId);
                if (error != ErrorCodes.NO_ERROR) {
                    LogWarning($"TraceId: { TraceId }, 'SendEmailUserSignUp', Not found user, user_id: { UserId }");
                    return;
                }
                if (user.VerifiedEmail) {
                    LogInformation($"TraceId: { TraceId }, User has verified email, user_id: { user.Id }");
                    return;
                }
                user.Settings.Remove("confirm_email");
                user.Settings.Add("confirm_email", new JObject(){
                    { "is_sending", true },
                });
                if (await __DBContext.SaveChangesAsync() <= 0) {
                    LogError($"TraceId: { TraceId }, 'SendEmailUserSignUp', Can't save changes before send email, user_id: { user.Id }");
                    return;
                }
                #region Send Email
                var urlConfirm = "";
                (urlConfirm, requestState) = Utils.GenerateUrlConfirm(user.Id, hostName, prefixUrl);
                var model = new UserSignUpEmailModel() {
                    UserName = user.UserName,
                    ConfirmLink = urlConfirm,
                };
                sendSuccess = await SendEmail<UserSignUpEmailModel>(new EmailForm() {
                    ToEmail = user.Email,
                    Subject = "[APP-NAME] Confirm signup.",
                    TraceId = TraceId,
                    Model = model,
                });
                #endregion
            }

            user.Settings.Remove("confirm_email");
            user.Settings.Add("confirm_email", new JObject(){
                { "is_sending", false },
                { "send_success", sendSuccess },
                { "send_date", DateTime.UtcNow.ToString(CommonDefine.DATE_TIME_FORMAT) },
                { "confirm_date", default },
                { "state", requestState },
            });
            if (await __DBContext.SaveChangesAsync() <= 0) {
                LogError($"TraceId: { TraceId }, 'SendEmailUserSignUp', Can't save changes after send email, user_id: { user.Id }");
                return;
            }
        }
        #endregion
    }
}
