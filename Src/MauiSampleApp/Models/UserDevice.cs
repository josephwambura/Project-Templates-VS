namespace MauiSampleApp.Models
{
    public class UserDevice : SyncableEntity
    {
        public Guid ApplicationUserId { get; set; }
        public string DeviceIdentifier { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;        // e.g., "John's iPhone"
        public string Model { get; set; } = string.Empty;             // e.g., "iPhone 15 Pro"
        public string Platform { get; set; } = string.Empty;          // e.g., "iOS"
        public bool IsEnabled { get; set; } = true;                  // Security toggle to revoke access

        // Navigation Property
        public ApplicationUser? User { get; set; }
    }
}