using System.Text.Json.Serialization;

namespace MauiSampleApp.Models
{
    public class ProjectTask : SyncableEntity
    {
        public string Title { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }

        [JsonIgnore]
        public Guid ProjectID { get; set; }
    }
}