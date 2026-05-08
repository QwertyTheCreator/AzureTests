using Azure.Core;
using Azure.Identity;

namespace AzureTests.Services;

public class AzureAuthProvider
{
    public TokenCredential GenerateToken()
    {
        DefaultAzureCredentialOptions options = new()
        {
            ExcludeEnvironmentCredential = true,
            ExcludeManagedIdentityCredential = true,
            ExcludeWorkloadIdentityCredential =  true
        };

        return Environment.GetEnvironmentVariable("Environment").ToLower() == "local"
            ? new DefaultAzureCredential(options)
            : new ManagedIdentityCredential();
    }
}