using System;
using FreshMvvm;
using Xamarin.Forms;
using Xamarin.Essentials;
using PropertyChanged;
using System.Threading.Tasks;

namespace ManageGo
{
    public class LoginPageModel : FreshBasePageModel
    {
        public bool IsBiometricsLabelVisible { get; private set; }
        public bool IsBioLoginVisible { get; private set; }
        public bool ResetPasswordViewIsVisible { get; private set; }
        public string BioLoginButtonText { get; private set; }
        public bool IsLoggingIn { get; private set; }

        internal event EventHandler<bool> OnSuccessfulLogin;
        public string AuthString { get; private set; }
        [AlsoNotifyFor("LoginButtonBgColor")]
        public string UserEmail { get; set; }
        [AlsoNotifyFor("LoginButtonBgColor")]
        public string UserPassword { get; set; }
        public string LoginButtonBgColor
        {
            get { return OnLoginButtonTapped.CanExecute(null) ? "#3E90F4" : "#d6e8ff"; }
        }
        public string AuthCheckBoxIcon
        {
            get { return IsBiometricsEnabled ? "checked.png" : "unchecked.png"; }
        }
        [AlsoNotifyFor("AuthCheckBoxIcon")]
        public bool IsBiometricsEnabled { get; private set; }
        public LoginPageModel()
        {
            var userName = Preferences.Get("UserName", "****");
            userName = userName.Substring(0, Math.Min(userName.Length - 1, 4)) + "****";
            LocalAuthType authType = DependencyService.Get<ILocalAuthHelper>().GetLocalAuthType();
            var isBioLoginEnabled = Preferences.Get("IsBiometricAuthEnabled", false);
            if (!isBioLoginEnabled)
            {
                switch (authType)
                {
                    case LocalAuthType.None:
                    case LocalAuthType.Passcode:
                        IsBiometricsLabelVisible = false;
                        break;
                    case LocalAuthType.FaceId:
                        IsBiometricsLabelVisible = true;
                        AuthString = "Turn on Face ID login";
                        break;
                    case LocalAuthType.NewAndroidBiometric:
                    case LocalAuthType.TouchId:
                        IsBiometricsLabelVisible = true;
                        AuthString = Device.RuntimePlatform == Device.iOS ? "Turn on Touch ID login" : "Turn on fingerprint login";
                        break;
                }
            }
            else if (authType == LocalAuthType.FaceId || authType == LocalAuthType.TouchId ||
                        authType == LocalAuthType.NewAndroidBiometric)
            {
                if (authType == LocalAuthType.FaceId)
                {
                    BioLoginButtonText = "Log in with Face ID";
                    IsBioLoginVisible = true;
                }
                else
                {
                    // we dont need to show this with the new API
                    BioLoginButtonText = Device.RuntimePlatform ==
                    Device.iOS ? "Log in with Touch ID"
                                : authType == LocalAuthType.NewAndroidBiometric ?
                                    "Log in with fingerprint" : $"Touch fingerprint sensor to log in as {userName}";
                    IsBioLoginVisible = true;
                }
                void onSuccess()
                {
                    async void action() =>
                           await FinishLogin(isBiometricLogin: true);
                    Device.BeginInvokeOnMainThread(action);
                }

                async void onFailure()
                {
                    //do nothing
                    // we dont need to show this with the new Android API 28
                    if (Device.RuntimePlatform == Device.Android && authType != LocalAuthType.NewAndroidBiometric)
                    {
                        await CoreMethods.DisplayAlert("ManageGo", "Fingerpring authentication failed", "DISMISS");
                    }
                }
                DependencyService.Get<ILocalAuthHelper>().Authenticate(userName, onSuccess, onFailure);
            }

        }

        public FreshAwaitCommand OnBioLoginButtonTapped
        {
            get
            {
                return new FreshAwaitCommand((tcs) =>
                {
                    if (Device.RuntimePlatform == Device.Android && DeviceInfo.Version < Version.Parse("9.0"))
                    {
                        return;
                    }
                    void onSuccess()
                    {
                        Device.BeginInvokeOnMainThread(async () =>
                            await FinishLogin(isBiometricLogin: true)
                        );
                    }
                    void onFailure()
                    {
                        //do nothing
                    }
                    var userName = Preferences.Get("UserName", "****").Substring(0, 4) + "****";
                    DependencyService.Get<ILocalAuthHelper>().Authenticate(userName, onSuccess, onFailure);
                    tcs?.SetResult(true);
                });
            }
        }

        public FreshAwaitCommand OnLoginButtonTapped
        {
            get
            {
                async void execute(TaskCompletionSource<bool> tcs)
                {
                    await FinishLogin(isBiometricLogin: false);
                    tcs?.SetResult(true);
                }
                return new FreshAwaitCommand(execute,
                        () => !string.IsNullOrWhiteSpace(UserEmail) && !string.IsNullOrWhiteSpace(UserPassword));
            }
        }

        private async Task FinishLogin(bool isBiometricLogin)
        {
            try
            {
                IsLoggingIn = true;
                if (Connectivity.NetworkAccess != NetworkAccess.Internet)
                    throw new Exception("Not connected to Internet");
                await Services.DataAccess.Login(UserEmail, UserPassword);
                Preferences.Set("IsFirstLogin", false);
                if (!isBiometricLogin)
                {
                    Preferences.Set("UserName", UserEmail);
                    Preferences.Set("Password", UserPassword);
                    Preferences.Set("IsBiometricAuthEnabled", IsBiometricsEnabled);
                }

                OnSuccessfulLogin?.Invoke(this, true);
            }
            catch (Exception ex)
            {
                await CoreMethods.DisplayAlert("ManageGo", ex.Message, "DISMISS");
            }
            finally
            {
                IsLoggingIn = false;
            }
        }

        public FreshAwaitCommand OnResetPasswordTapped
        {
            get
            {
                void execute(System.Threading.Tasks.TaskCompletionSource<bool> tcs)
                {
                    ResetPasswordViewIsVisible = true;
                    tcs?.SetResult(true);
                }
                return new FreshAwaitCommand(execute);
            }
        }

        public FreshAwaitCommand OnCancelResetPasswordTapped
        {
            get
            {
                void execute(System.Threading.Tasks.TaskCompletionSource<bool> tcs)
                {
                    ResetPasswordViewIsVisible = false;
                    tcs?.SetResult(true);
                }
                return new FreshAwaitCommand(execute);
            }
        }

        public FreshAwaitCommand OnSubmitResetPasswordTapped
        {
            get
            {
                async void execute(System.Threading.Tasks.TaskCompletionSource<bool> tcs)
                {
                    if (string.IsNullOrWhiteSpace(UserEmail))
                    {
                        await CoreMethods.DisplayAlert("ManageGo", "Enter your email to reset password", "OK");
                        tcs?.SetResult(true);
                        return;
                    }
                    try
                    {
                        await Services.DataAccess.ResetPassword(UserEmail);
                        ResetPasswordViewIsVisible = false;
                        await CoreMethods.DisplayAlert("ManageGo", "We have sent you an email with your password reset instructions.", "DISMISS");
                    }
                    catch (Exception ex)
                    {
                        await CoreMethods.DisplayAlert("Something went wrong", ex.Message, "DISMISS");

                    }
                    finally
                    {
                        tcs?.SetResult(true);
                    }

                }
                return new FreshAwaitCommand(execute);
            }
        }

        public FreshAwaitCommand OnEnableBioAuthTapped
        {
            get
            {
                return new FreshAwaitCommand((tcs) =>
                {
                    IsBiometricsEnabled = !IsBiometricsEnabled;
                    tcs?.SetResult(true);
                });
            }
        }


    }
}
