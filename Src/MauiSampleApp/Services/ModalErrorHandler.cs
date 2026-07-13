namespace MauiSampleApp.Services
{
    /// <summary>
    /// Modal Error Handler.
    /// </summary>
    public class ModalErrorHandler(IFingerprint fingerprint) : IErrorHandler
    {
        readonly SemaphoreSlim _semaphore = new(1, 1);

        /// <summary>
        /// Handle error in UI.
        /// </summary>
        /// <param name="ex">Exception.</param>
        public void HandleError(Exception ex)
        {
            HandleError("Error", ex, "OK");
        }

        /// <summary>
        /// Handle error in UI.
        /// </summary>
        /// <param name="ex">Exception.</param>
        public void HandleError(string title, Exception ex, string cancel)
        {
            DisplayAlertAsync(title, ex, cancel).FireAndForgetSafeAsync();
        }

        async Task DisplayAlertAsync(string title, Exception ex, string cancel)
        {
            try
            {
                await _semaphore.WaitAsync();
                if (Shell.Current is Shell shell)
                    await shell.DisplayAlertAsync(title, ex.Message, cancel);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<bool> AuthenticateBiometrics(string title = "Prove you have fingers!", string reason = "Because without it you can't have access")
        {
            var config = new AuthenticationRequestConfiguration(
                title: title,
                reason: reason)
            {
                CancelTitle = "Cancel",
                FallbackTitle = "Use PIN",
                AllowAlternativeAuthentication = true,  // allows PIN/password fallback
            };

            // Check availability before prompting (optional but recommended)
            var availability = await fingerprint.GetAvailabilityAsync(
                allowAlternativeAuthentication: config.AllowAlternativeAuthentication);

            if (availability != FingerprintAvailability.Available)
            {
                HandleError("Unavailable", new Exception($"Biometric auth not available: {availability}"), "OK");
                return false;
            }

            var result = await fingerprint.AuthenticateAsync(config);

            if (result.Authenticated)
            {
                // success — navigate or unlock
                return true;
            }
            else
            {
                HandleError("Failed", new Exception(result.ErrorMessage ?? result.Status.ToString()), "OK");
                return false;
            }
        }
    }
}