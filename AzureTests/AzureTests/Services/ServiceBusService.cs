using Azure.Messaging.ServiceBus;

namespace AzureTests.Services;

public class ServiceBusService(AzureAuthProvider azureAuthProvider, ILogger<ServiceBusService> logger)
{
    private ServiceBusClient CreateBusClient()
    {
        // Using SAS auth
        var svbConnectionString = Environment.GetEnvironmentVariable("SVB_ConnectionString");
        return new ServiceBusClient(svbConnectionString);
    }

    //Supports only string format due testing/learning purpose
    public async Task SendMessage(string message, string queueName)
    {
        var sender = CreateBusClient().CreateSender(queueName);

        try
        {
            var svbMessage = new ServiceBusMessage(message);
            await sender.SendMessageAsync(svbMessage);
        }
        catch (Exception ex)
        {
            logger.LogError("Error while trying to send a message: {Message}", ex.Message);
        }
        finally
        {
            await sender.DisposeAsync();
        }
    }

    public async Task<List<string>> ReceiveUnreadMessages(string queueName)
    {
        var receiverOption = new ServiceBusReceiverOptions();
        receiverOption.ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete;
        
        var receiver = CreateBusClient().CreateReceiver(queueName, receiverOption);

        var messageStrings = (await receiver.ReceiveMessagesAsync(5)).Select(message => message.Body.ToString()).ToList();

        return messageStrings;
    }
}