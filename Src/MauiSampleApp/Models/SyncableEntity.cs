namespace MauiSampleApp.Models
{
    public abstract class SyncableEntity
    {
        // Global UUIDs prevent primary key collisions when clients generate data offline
        public Guid ID { get; set; } = Guid.Empty;

        // The UTC timestamp when this row was modified (Client-side or Server-side)
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

        // Set to true when modified locally. Cleared only when server explicitly acknowledges receipt
        public bool IsClientDirty { get; set; } = true;

        // True if the record was deleted. Used to propagate deletes to the server
        public bool IsDeleted { get; set; } = false;

        // Version token (concurrency stamp / revision epoch) assigned by the server
        public long ServerVersion { get; set; } = 0;
    }
}