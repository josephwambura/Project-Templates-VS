namespace MauiSampleApp.Models
{
    public class ApplicationUser : SyncableEntity
    {
        public string Username { get; set; } = string.Empty;
        public string UsernameNormalized { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty; // Note: For production use, apply a hashing algorithm!

        // Navigation Property: A single user can have multiple authorized devices bound
        public List<UserDevice> Devices { get; set; } = [];
    }
}