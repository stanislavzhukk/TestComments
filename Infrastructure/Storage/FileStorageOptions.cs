namespace Infrastructure.Storage
{
    public class FileStorageOptions
    {
        public const string SectionName = "FileStorage";
        public string UploadsPath { get; init; } = "uploads";
    }
}
