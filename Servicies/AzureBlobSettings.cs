namespace InvestNaijaAuth.Servicies
{
    public class AzureBlobSettings
    {
        public required string AccountName { get; set; }
        public required string AccountKey { get; set; }
        public required string ContainerName { get; set; }
    }
}
