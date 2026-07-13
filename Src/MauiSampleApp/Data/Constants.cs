using System.Security.Cryptography;

namespace MauiSampleApp.Data
{
    public static class Constants
    {
        public const string DatabaseFilename = "AppSQLite.db3";

        public static string DatabasePath =>
            $"Data Source={Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename)}";

        public static string BackendBaseAddress => "https://api.yourdomain.com/";

        public static int Pbkdf2Iterations => 100_000;

        public static HashAlgorithmName HashAlgorithm => HashAlgorithmName.SHA3_256;
    }
}