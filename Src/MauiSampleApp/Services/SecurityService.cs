namespace MauiSampleApp.Services
{
    public class SecurityService(
        IFingerprint fingerprint,
        UserRepository userRepository,
        IErrorHandler errorHandler) : ISecurityService
    {
        public async Task<bool> AuthenticateWithFallback(Guid currentUserId, string title = "Verify Identity", string reason = "Please authenticate to proceed.")
        {
            var availability = await fingerprint.GetAvailabilityAsync(allowAlternativeAuthentication: true);

            if (availability == FingerprintAvailability.Available)
            {
                var result = await AuthenticateBiometrics(availability, title, reason);
                if (result) return true;
            }

            return await PromptAndVerifyPinAsync(currentUserId);
        }

        private async Task<bool> AuthenticateBiometrics(FingerprintAvailability availability, string title, string reason)
        {
            var config = new AuthenticationRequestConfiguration(title, reason)
            {
                CancelTitle = "Cancel",
                FallbackTitle = "Use PIN",
                AllowAlternativeAuthentication = true,
            };

            if (availability != FingerprintAvailability.Available)
            {
                await errorHandler.DisplayAlertAsync("Unavailable", $"Biometric auth not available: {availability}");
                return false;
            }

            var result = await fingerprint.AuthenticateAsync(config);

            if (result.Authenticated)
            {
                return true;
            }
            else
            {
                // Gracefully log fail but do not lock thread
                await errorHandler.DisplayAlertAsync("Failed", result.ErrorMessage ?? result.Status.ToString());
                return false;
            }
        }

        private async Task<bool> PromptAndVerifyPinAsync(Guid userId)
        {
            var user = await userRepository.GetAsync(userId);
            if (user == null) return false;

            if (string.IsNullOrWhiteSpace(user.PinHash))
            {
                return await RegisterNewPinAsync(user);
            }

            string enteredPin = await DisplayPromptOnActivePage(
                title: "App PIN Required",
                message: "Please enter your security PIN to authorize this action.",
                accept: "Submit",
                cancel: "Cancel",
                placeholder: "4 to 6 digit PIN",
                maxLength: 6,
                keyboard: Keyboard.Numeric);

            if (string.IsNullOrWhiteSpace(enteredPin)) return false;

            bool isValid = user.PinHash == enteredPin.Trim();

            if (!isValid)
            {
                await errorHandler.DisplayAlertAsync("Incorrect PIN", "The security PIN entered is incorrect.");
            }

            return isValid;
        }

        private async Task<bool> RegisterNewPinAsync(Models.ApplicationUser user)
        {
            await errorHandler.DisplayAlertAsync("Setup PIN", "Biometrics are unavailable. Let's configure a fallback security PIN.");

            string newPin = await DisplayPromptOnActivePage("Create PIN", "Enter a 4-6 digit PIN:", keyboard: Keyboard.Numeric);
            if (string.IsNullOrWhiteSpace(newPin) || newPin.Length < 4)
            {
                await errorHandler.DisplayAlertAsync("Invalid PIN", "PIN must be between 4 and 6 digits.");
                return false;
            }

            string confirmPin = await DisplayPromptOnActivePage("Confirm PIN", "Re-enter your PIN:", keyboard: Keyboard.Numeric);
            if (newPin != confirmPin)
            {
                await errorHandler.DisplayAlertAsync("Mismatch", "PINs do not match. Setup aborted.");
                return false;
            }

            user.PinHash = newPin;
            await userRepository.SaveItemAsync(user);

            await errorHandler.DisplayAlertAsync("Success", "Security PIN configured successfully!");
            return true;
        }

        private async Task<string> DisplayPromptOnActivePage(
            string title,
            string message,
            string accept = "OK",
            string cancel = "Cancel",
            string placeholder = "",
            int maxLength = -1,
            Keyboard? keyboard = null)
        {
            if (Shell.Current is Shell shell)
            {
                return await shell.DisplayPromptAsync(title, message, accept, cancel, placeholder, maxLength, keyboard);
            }
            else if (Application.Current?.Windows.FirstOrDefault()?.Page is Page activePage)
            {
                return await activePage.DisplayPromptAsync(title, message, accept, cancel, placeholder, maxLength, keyboard);
            }

            return string.Empty;
        }
    }
}