namespace MauiSampleApp.Services
{
    public interface IEnterpriseSyncEngine
    {
        Task TriggerSyncAsync();
        void InitializeMonitoring();
        void ShutdownMonitoring();
    }
}