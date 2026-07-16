namespace MauiSampleApp.Services
{
    /// <summary>
    /// Domain Service handling local biometric and PIN security configurations.
    /// </summary>
    public interface ISecurityService
    {
        /// <summary>
        /// Authenticate using biometrics. If unavailable, falls back automatically to prompting for the local App PIN.
        /// </summary>
        Task<bool> AuthenticateWithFallback(Guid currentUserId, string title = "Verify Identity", string reason = "Please authenticate to proceed.");
    }
}