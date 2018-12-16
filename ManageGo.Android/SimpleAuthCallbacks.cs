using System;
using Android.Hardware.Fingerprints;
using Android.Support.V4.Hardware.Fingerprint;
using ManageGo.Droid;
using Xamarin.Forms;
using Javax.Crypto;
using Java.Lang;

[assembly: Dependency(typeof(LocalAuthHelper))]
namespace ManageGo.Droid
{
    class SimpleAuthCallbacks : FingerprintManagerCompat.AuthenticationCallback
    {
        // ReSharper disable once MemberHidesStaticFromOuterClass
        static readonly string TAG = "X:" + typeof(SimpleAuthCallbacks).Name;
        static readonly byte[] SECRET_BYTES = { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        LocalAuthHelper owner;

        public SimpleAuthCallbacks(LocalAuthHelper _owner)
        {
            owner = _owner;
        }

        public override void OnAuthenticationSucceeded(FingerprintManagerCompat.AuthenticationResult result)
        {
            if (result.CryptoObject?.Cipher != null)
            {
                try
                {
                    // Calling DoFinal on the Cipher ensures that the encryption worked.
                    byte[] doFinalResult = result.CryptoObject.Cipher.DoFinal(SECRET_BYTES);
                    ReportSuccess();
                }
                catch (BadPaddingException)
                {

                    ReportAuthenticationFailed();
                }
                catch (IllegalBlockSizeException)
                {

                    ReportAuthenticationFailed();
                }
            }
            else
            {
                // No cipher used, assume that everything went well and trust the results.

                ReportSuccess();
            }
        }

        void ReportSuccess()
        {
            owner.OnSuccess?.Invoke();
        }

        void ReportScanFailure(int errMsgId, string errorMessage)
        {
            owner.OnFailure?.Invoke();
        }

        void ReportAuthenticationFailed()
        {
            owner.OnFailure?.Invoke();
        }

        public override void OnAuthenticationError(int errMsgId, ICharSequence errString)
        {
            // There are some situations where we don't care about the error. For example, 
            // if the user cancelled the scan, this will raise errorID #5. We don't want to
            // report that, we'll just ignore it as that event is a part of the workflow.
            bool reportError = (errMsgId == (int)FingerprintState.ErrorCanceled);
            string debugMsg = string.Format("OnAuthenticationError: {0}:`{1}`.", errMsgId, errString);
            if (reportError)
            {
                ReportScanFailure(errMsgId, errString.ToString());
                debugMsg += " Reporting the error.";
            }
            else
            {
                debugMsg += " Ignoring the error.";
            }
        }


        public override void OnAuthenticationFailed()
        {

            ReportAuthenticationFailed();
        }

        public override void OnAuthenticationHelp(int helpMsgId, ICharSequence helpString)
        {

            ReportScanFailure(helpMsgId, helpString.ToString());
        }
    }

}
