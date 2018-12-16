using System;
using Android.Support.V4.Hardware.Fingerprint;
using ManageGo.Droid;
using Xamarin.Forms;
using Plugin.CurrentActivity;
using Android.Security.Keystore;
using Android.Hardware.Biometrics;
using Android.Runtime;
using Java.Lang;
using Android.OS;
using Android.Content;

[assembly: Dependency(typeof(LocalAuthHelper))]
namespace ManageGo.Droid
{
    public class LocalAuthHelper : ILocalAuthHelper
    {

        private CancellationSignal newCancelSignal;
        private Android.Support.V4.OS.CancellationSignal cancellationSignal;
        private BiometricPrompt prompt;
        public Action OnSuccess;
        public Action OnFailure;
        static readonly string KEY_ALGORITHM = KeyProperties.KeyAlgorithmAes;
        static readonly string BLOCK_MODE = KeyProperties.BlockModeCbc;
        static readonly string ENCRYPTION_PADDING = KeyProperties.EncryptionPaddingPkcs7;
        public bool IsBiometricPromptEnabled
        {
            get
            {
                return (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.P);
            }
        }
        static readonly string TRANSFORMATION = KEY_ALGORITHM + "/" +
                                                BLOCK_MODE + "/" +
                                                ENCRYPTION_PADDING;
        public FingerprintManagerCompat FingerprintManager { get; private set; }

        public LocalAuthHelper()
        {

            FingerprintManager = FingerprintManagerCompat.From(context: CrossCurrentActivity.Current.AppContext);


        }

        internal void CancelAuthentication()
        {
            cancellationSignal?.Cancel();
            newCancelSignal?.Cancel();
            newCancelSignal = null;
            cancellationSignal = null;
        }


        public void Authenticate(string userId, Action onSuccess, Action onFailure)
        {
            OnSuccess = onSuccess;
            OnFailure = onFailure;
            if (IsBiometricPromptEnabled)
            {
                newCancelSignal = new Android.OS.CancellationSignal();
                prompt = new BiometricPrompt.Builder(CrossCurrentActivity.Current.Activity)
                    .SetTitle("ManageGo")
                    .SetSubtitle("Fingerprint Login")
                    .SetDescription($"Place finger on the home button to log in as {userId}")
                    .SetNegativeButton("CANCEL", CrossCurrentActivity.Current.Activity.MainExecutor,
                        new DialogListener(this)).Build();

                prompt.Authenticate(newCancelSignal, CrossCurrentActivity.Current.Activity.MainExecutor, new BiometricCallback(this));
            }
            else
            {
                cancellationSignal = new Android.Support.V4.OS.CancellationSignal();
                var callback = new SimpleAuthCallbacks(this);
                FingerprintManager.Authenticate(null, 0, cancellationSignal, callback, null);
            }

        }

        public LocalAuthType GetLocalAuthType()
        {

            if (FingerprintManager is null || !FingerprintManager.IsHardwareDetected ||
                    !FingerprintManager.HasEnrolledFingerprints)
                return LocalAuthType.None;
            else if (IsBiometricPromptEnabled)
                return LocalAuthType.NewAndroidBiometric;
            else
                return LocalAuthType.TouchId;
        }
    }

    public class BiometricCallback : BiometricPrompt.AuthenticationCallback
    {
        readonly LocalAuthHelper owner;
        public BiometricCallback(LocalAuthHelper _owner)
        {
            owner = _owner;
        }
        public override void OnAuthenticationSucceeded(BiometricPrompt.AuthenticationResult result)
        {
            owner.OnSuccess?.Invoke();
        }

        public override void OnAuthenticationFailed()
        {
            owner.OnFailure?.Invoke();
        }
        public override void OnAuthenticationHelp([GeneratedEnum] BiometricAcquiredStatus helpCode, ICharSequence helpString)
        {
            base.OnAuthenticationHelp(helpCode, helpString);
        }
    }

    public class DialogListener : Java.Lang.Object, IDialogInterfaceOnClickListener
    {
        readonly LocalAuthHelper owner;
        public DialogListener(LocalAuthHelper _owner)
        {
            owner = _owner;
        }

        public void OnClick(IDialogInterface dialog, int which)
        {
            owner.CancelAuthentication();
        }
    }

}
