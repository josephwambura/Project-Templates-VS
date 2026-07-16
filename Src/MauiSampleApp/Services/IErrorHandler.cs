namespace MauiSampleApp.Services
{
    /// <summary>
    /// Error Handler Service contract.
    /// </summary>
    public interface IErrorHandler
    {
        /// <summary>
        /// Handle error in UI with default title.
        /// </summary>
        void HandleError(Exception ex);

        /// <summary>
        /// Handle error in UI with custom configurations.
        /// </summary>
        void HandleError(string title, Exception ex, string cancel = "OK");

        /// <summary>
        /// Direct, safe access to alert UI without wrapping in an Exception.
        /// </summary>
        Task DisplayAlertAsync(string title, string message, string cancel = "OK");
    }
}