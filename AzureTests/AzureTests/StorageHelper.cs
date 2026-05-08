using Azure.Core;
using Azure.Identity;
using Azure.Storage.Blobs;

namespace AzureTests;

public static class StorageHelper
{
    public static BlobServiceClient GetClient()
    {
        var azStorageAccount = Environment.GetEnvironmentVariable("AZURESTORAGE_ACCOUNT") ?? "undefinedAccountName";
        
        
        DefaultAzureCredentialOptions options = new()
        {
            ExcludeEnvironmentCredential = true,
            ExcludeManagedIdentityCredential = true,
            ExcludeWorkloadIdentityCredential =  true
        };

        TokenCredential credential = Environment.GetEnvironmentVariable("Environment").ToLower() == "local"
            ? new DefaultAzureCredential(options)
            : new ManagedIdentityCredential();
    
        string blobServiceEndpoint = $"https://{azStorageAccount}.blob.core.windows.net";
        return new BlobServiceClient(new Uri(blobServiceEndpoint), credential);
    }
}