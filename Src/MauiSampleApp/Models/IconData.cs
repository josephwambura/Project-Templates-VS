using System;

namespace MauiSampleApp.Models
{
    public class IconData
    {
        public string? Icon { get; set; }
        public string? Description { get; set; }
    }

    public class SyncBatchRequest
    {
        // The timestamp or version watermark of the client's last successful execution
        public long LastClientVersion { get; set; }
        public List<SyncItemDto> Changes { get; set; } = [];
    }

    public class SyncBatchResponse
    {
        // The absolute highest engine sync version currently stamped on the server
        public long NewServerVersion { get; set; }

        // List of new changes the client must pull down and apply
        public List<SyncItemDto> ServerChanges { get; set; } = [];

        // List of client primary keys successfully acknowledged and merged by the server
        public List<Guid> AcknowledgedIds { get; set; } = [];
    }

    public class SyncItemDto
    {
        public Guid Id { get; set; }
        public string EntityType { get; set; } = string.Empty; // e.g., "Project", "Task"
        public string JsonData { get; set; } = string.Empty;   // Serialized state of the entity
        public DateTimeOffset UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public long ServerVersion { get; set; }
    }
}
