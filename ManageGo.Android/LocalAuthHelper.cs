using System;
using Android.Hardware.Fingerprints;
using Android.Support.V4.Hardware.Fingerprint;
using ManageGo.Droid;
using Xamarin.Forms;
using Plugin.CurrentActivity;
using Android.Support.V4.Content;
using Android;
using Android.Content.PM;
using Android.Support.V4.OS;
using Javax.Crypto;
using Android.Security.Keystore;

[assembly: Dependency(typeof(LocalAuthHelper))]
namespace ManageGo.Droid
{
    public class LocalAuthHelper : ILocalAuthHelper
    {
        private CancellationSignal _cancellationSignal;
        static readonly string KEY_ALGORITHM = KeyProperties.KeyAlgorithmAes;
        static readonly string BLOCK_MODE = KeyProperties.BlockModeCbc;
        static readonly string ENCRYPTION_PADDING = KeyProperties.EncryptionPaddingPkcs7;
        public event EventHandler AuthenticationSucceded;
        static readonly string TRANSFORMATION = KEY_ALGORITHM + "/" +
                                                BLOCK_MODE + "/" +
                                                ENCRYPTION_PADDING;
        public FingerprintManagerCompat FingerprintManager { get; private set; }

        public LocalAuthHelper()
        {
            Permission permissionResult = ContextCompat.CheckSelfPermission(CrossCurrentActivity.Current.AppContext,
                                                                                  Manifest.Permission.UseBiometric);
            if (permissionResult == Permission.Granted)
            {
                FingerprintManager = FingerprintManagerCompat.From(context: CrossCurrentActivity.Current.AppContext);
            }
        }

        public void CancelAuthentication()
        {
            _cancellationSignal.Cancel();
            _cancellationSignal = null;
        }


        public void Authenticate(string userId, Action onSuccess, Action onFailure)
        {
            if (FingerprintManager is null)
            {
                onFailure?.Invoke();
                return;
            }
            _cancellationSignal = new CancellationSignal();
            Cipher cipher = Cipher.GetInstance(TRANSFORMATION);
            var callback = new SimpleAuthCallbacks();
            callback.AuthenticationSucceded += (object sender, EventArgs e) =>
            {
                onSuccess?.Invoke();
            };
            callback.AuthenticationFailed += (sender, e) =>
            {
                onFailure?.Invoke();
            };
            FingerprintManager.Authenticate(null,
                (int)FingerprintAuthenticationFlags.None,
                _cancellationSignal, callback, null
            );
        }

        public LocalAuthType GetLocalAuthType()
        {
            if (!FingerprintManager.IsHardwareDetected || !FingerprintManager.HasEnrolledFingerprints)
                return LocalAuthType.None;
            else
                return LocalAuthType.TouchId;
        }
    }

}
