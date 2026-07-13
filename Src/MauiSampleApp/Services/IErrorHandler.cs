namespace MauiSampleApp.Services
{
    /// <summary>
    /// Error Handler Service.
    /// </summary>
    public interface IErrorHandler
    {
        /// <summary>
        /// Handle error in UI.
        /// </summary>
        /// <param name="ex">Exception being thrown.</param>
        void HandleError(Exception ex);

        /// <summary>
        /// Handle error in UI.
        /// </summary>
        /// <param name="title">Title of the error.</param>
        /// <param name="ex">Exception being thrown.</param>
        /// <param name="cancel">Cancel button text.</param>
        void HandleError(string title, Exception ex, string cancel);

        /// <summary>
        /// Authenticate user using biometrics (fingerprint, face recognition, etc.).
        /// </summary>
        /// <param name="title"></param>
        /// <param name="reason"></param>
        /// <returns></returns>
        Task<bool> AuthenticateBiometrics(string title = "Prove you have fingers!", string reason = "Because without it you can't have access");
    }
}