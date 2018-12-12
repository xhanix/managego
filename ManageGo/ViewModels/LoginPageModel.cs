using System;
using FreshMvvm;
using Xamarin.Forms;
using Xamarin.Essentials;
using PropertyChanged;

namespace ManageGo
{
    public class LoginPageModel : FreshBasePageModel
    {
        public bool IsBiometricsLabelVisible { get; private set; }
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
            if (Device.RuntimePlatform == Device.Android)
                return;
            var authType = DependencyService.Get<ILocalAuthHelper>().GetLocalAuthType();
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
                case LocalAuthType.TouchId:
                    IsBiometricsLabelVisible = true;
                    AuthString = "Turn on Touch ID login";
                    break;
            }
        }

        public FreshAwaitCommand OnLoginButtonTapped
        {
            get
            {
                return new FreshAwaitCommand((tcs) =>
                {

                }, () => !string.IsNullOrWhiteSpace(UserEmail) && !string.IsNullOrWhiteSpace(UserPassword));
            }
        }

        public FreshAwaitCommand OnResetPasswordTapped
        {
            get
            {
                return new FreshAwaitCommand(async (tcs) =>
                {
                    if (string.IsNullOrWhiteSpace(UserEmail))
                    {
                        await CoreMethods.DisplayAlert("ManageGo", "Enter email to reset password", "DISMISS");
                        tcs?.SetResult(true);
                        return;
                    }
                    tcs?.SetResult(true);
                });
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
