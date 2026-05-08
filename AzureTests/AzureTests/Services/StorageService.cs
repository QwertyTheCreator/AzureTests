using Azure.Core;
using Azure.Identity;
using Azure.Storage.Blobs;
using AzureTests.Services;

namespace AzureTests;

public class StorageService(AzureAuthProvider azureAuthProvider)
{
    public BlobServiceClient GetClient()
    {
        var credential = azureAuthProvider.GenerateToken();
        var azStorageAccount = Environment.GetEnvironmentVariable("AZURESTORAGE_ACCOUNT") ?? "undefinedAccountName";
        
        string blobServiceEndpoint = $"https://{azStorageAccount}.blob.core.windows.net";
        
        return new BlobServiceClient(new Uri(blobServiceEndpoint), credential);
    }
}