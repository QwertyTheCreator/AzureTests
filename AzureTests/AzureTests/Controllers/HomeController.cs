using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AzureTests.Models;

namespace AzureTests.Controllers;

public class HomeController(ILogger<HomeController> logger) : Controller
{
    public async Task<IActionResult> Index()
    {
        logger.LogInformation("User just reached HomePage");

        var blobServiceClient = StorageHelper.GetClient();

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

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}