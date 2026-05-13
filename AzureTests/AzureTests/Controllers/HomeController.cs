using System.Diagnostics;
using AzureTests.Models;
using AzureTests.Services;
using Microsoft.AspNetCore.Mvc;

namespace AzureTests.Controllers;

public class HomeController(
    ILogger<HomeController> logger,
    ServiceBusService busService,
    StorageService storageService) : Controller
{
    public async Task<IActionResult> Index()
    {
        await SendLogToStorageQueue("User entered Index page");
        
        var ip = await SendMessageWithClientIp();
        
        logger.LogInformation("User {ip} reached HomePage", ip);

        var blobServiceClient = storageService.GetClient();

        var containers = await blobServiceClient.GetBlobContainersAsync().ToListAsync();
        var containerNames = string.Join(",\n", containers.Select(c => c.Name));

        var env = Environment.GetEnvironmentVariable("Environment") ?? "undefined";

        var container = containers.ElementAt(Random.Shared.Next(0, containers.Count()));
        var containerClient = blobServiceClient.GetBlobContainerClient(container.Name);

        var props = container.Properties;
        var textToRepresent = $"Environment {env}" + "\n" +
                              $" ContainerNames: {containerNames}\n" +
                              $" Properties for {container.Name}, {containerClient.Uri}: PublicAccess - {props.PublicAccess},\n" +
                              $"Last Modified - {props.LastModified}";
        return View("Index", textToRepresent);
    }

    public async Task<IActionResult> RecentIps()
    {
        await SendLogToStorageQueue("User entered RecentIps page");
        
        var unreadMessages = await busService.ReceiveUnreadMessages(Constants.ServiceBusQueues.TestQueue);
        
        return View("RecentIps", unreadMessages);
    }
    
    public async Task<IActionResult> RecentLogs()
    {
        await SendLogToStorageQueue("User entered RecentLogs page");

        var unreadMessages = await ReceiveMessagesFromStorageQueue(10);
        
        return View("RecentLogs", unreadMessages.ToList());
    }
    
    private async Task<string> SendMessageWithClientIp()
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "<undefined>";
        await busService.SendMessage(ip, Constants.ServiceBusQueues.TestQueue);

        return ip;
    }

    private async Task SendLogToStorageQueue(string message)
    {
        var client = await storageService.CreateQueueIfNotExists();
        await client.SendMessageAsync(message);
    }

    private async Task<IEnumerable<string>> ReceiveMessagesFromStorageQueue(int count)
    {
        var client = await storageService.CreateQueueIfNotExists();
        return await storageService.ReceiveUnreadMessages(client, count);
    }
    
    public async Task<IActionResult> Privacy()
    {
        await SendLogToStorageQueue("User entered Privacy page");
        
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}