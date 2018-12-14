using System;
using Foundation;
using LocalAuthentication;
using ManageGo.iOS;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(LocalAuthHelper))]
namespace ManageGo.iOS
{
    public class LocalAuthHelper : ILocalAuthHelper
    {


        public string GetLocalAuthIcon()
        {
            var localAuthType = GetLocalAuthType();
            switch (localAuthType)
            {
                case LocalAuthType.Passcode:
                    return "SvgLibrary.LockIcon";
                case LocalAuthType.TouchId:
                    return "SvgLibrary.TouchIdIcon";
                case LocalAuthType.FaceId:
                    return "SvgLibrary.FaceIdIcon";
                default:
                    return string.Empty;
            }
        }

        public string GetLocalAuthUnlockText()
        {
            var localAuthType = GetLocalAuthType();

            switch (localAuthType)
            {
                case LocalAuthType.Passcode:
                    return "UnlockWithPasscode";
                case LocalAuthType.TouchId:
                    return "UnlockWithTouchID";
                case LocalAuthType.FaceId:
                    return "UnlockWithFaceID";
                default:
                    return string.Empty;
            }
        }

        public bool IsLocalAuthAvailable => GetLocalAuthType() != LocalAuthType.None;

        public void Authenticate(string userId, Action onSuccess, Action onFailure)
        {
            var context = new LAContext();
            if (context.CanEvaluatePolicy(LAPolicy.DeviceOwnerAuthenticationWithBiometrics, out NSError AuthError)
                || context.CanEvaluatePolicy(LAPolicy.DeviceOwnerAuthentication, out AuthError))
            {
                var replyHandler = new LAContextReplyHandler((success, error) =>
                {
                    if (success)
                    {
                        onSuccess?.Invoke();
                    }
                    else
                    {
                        onFailure?.Invoke();
                    }
                });

                context.EvaluatePolicy(LAPolicy.DeviceOwnerAuthentication, $"Sign in with Online Id for {userId}", replyHandler);
            }
        }

        public LocalAuthType GetLocalAuthType()
        {
            var localAuthContext = new LAContext();
            if (localAuthContext.CanEvaluatePolicy(LAPolicy.DeviceOwnerAuthentication, out NSError AuthError))
            {
                if (localAuthContext.CanEvaluatePolicy(LAPolicy.DeviceOwnerAuthenticationWithBiometrics, out AuthError))
                {
                    if (GetOsMajorVersion() >= 11 && localAuthContext.BiometryType == LABiometryType.FaceId)
                    {
                        return LocalAuthType.FaceId;
                    }
                    return LocalAuthType.TouchId;
                }
                return LocalAuthType.Passcode;
            }
            return LocalAuthType.None;
        }

        private int GetOsMajorVersion()
        {
            return int.Parse(UIDevice.CurrentDevice.SystemVersion.Split('.')[0]);
        }
    }
}
