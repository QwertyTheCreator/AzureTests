using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AzureTests.Models;

namespace AzureTests.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        var env = Environment.GetEnvironmentVariable("Environment") ?? "undefined";
        return View("Index", env);
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