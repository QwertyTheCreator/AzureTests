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

        var textToRepresent = $"Environment {env}" + "\n" +
                              $" ContainerNames: {containerNames}";
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