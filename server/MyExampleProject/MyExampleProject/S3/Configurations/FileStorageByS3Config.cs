namespace MyExampleProject.S3.Configurations
{
    public class FileStorageByS3Config
    {
        public bool IsActive { get; set; } = false;
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string Bucket { get; set; }
    }
}
