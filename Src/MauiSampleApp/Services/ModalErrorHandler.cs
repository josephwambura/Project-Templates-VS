namespace MauiSampleApp.Services
{
    /// <summary>
    /// Modal Error Handler targeting active app layout targets.
    /// </summary>
    public class ModalErrorHandler : IErrorHandler
    {
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public void HandleError(Exception ex)
        {
            HandleError("Error", ex, "OK");
        }

        public void HandleError(string title, Exception ex, string cancel = "OK")
        {
            // Fire-and-forget safely across threads
            DisplayAlertAsync(title, ex?.Message ?? "An unknown error occurred.", cancel).FireAndForgetSafeAsync();
        }

        public async Task DisplayAlertAsync(string title, string message, string cancel = "OK")
        {
            try
            {
                await _semaphore.WaitAsync();

                if (Shell.Current is Shell shell)
                {
                    await shell.DisplayAlertAsync(title, message, cancel);
                }
                else if (Application.Current?.Windows.FirstOrDefault()?.Page is Page activePage)
                {
                    await activePage.DisplayAlertAsync(title, message, cancel);
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}