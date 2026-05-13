using Azure.Core;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
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

    public async Task<QueueClient> CreateQueueIfNotExists(string queueName = Constants.ServiceBusQueues.TestQueue)
    {
        var connectionString = Environment.GetEnvironmentVariable("STORAGE_ConncetionString");

        var client = new QueueClient(connectionString, queueName);
        await client.CreateIfNotExistsAsync();

        return client;
    }

    public async Task SendMessageToQueue(QueueClient client, string message)
    {
        if (!await client.ExistsAsync())
        {
            return;
        }
        
        await client.SendMessageAsync(message);
    }

    public async Task<IEnumerable<string>> ReceiveUnreadMessages(QueueClient client, int maxCount)
    {
        if (!await client.ExistsAsync())
        {
            return [];
        }

        var messages = (await client.ReceiveMessagesAsync(maxCount)).Value;
        
        var tasks = new List<Task>();
        foreach (var message in messages)
        {
            var deleteTask = client.DeleteMessageAsync(message.MessageId, message.PopReceipt);
            tasks.Add(deleteTask);
        }
        await Task.WhenAll(tasks);
        
        return messages.Select(message => message.MessageText);
    }
}