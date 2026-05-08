using Azure.Identity;
using Azure.Storage.Blobs;
using AzureTests;
using AzureTests.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<AzureAuthProvider>();
builder.Services.AddSingleton<StorageService>();
builder.Services.AddSingleton<ServiceBusService>();

builder.Logging.AddAzureWebAppDiagnostics();
builder.Logging.AddConsole();

var app = builder.Build();

var env = Environment.GetEnvironmentVariable("Environment") ?? "undefined";
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//await SetUpStorage(env);

app.UseHttpsRedirection();
app.UseRouting();
app.Logger.LogInformation("Application started");
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();

static async Task SetUpStorage(string env = "undefined")
{
    env = env.ToLower();
    var rng = Random.Shared.Next();
    var azStorageAccount = Environment.GetEnvironmentVariable("AZURESTORAGE_ACCOUNT") ?? "undefinedname";
    DefaultAzureCredentialOptions options = new()
    {
        ExcludeEnvironmentCredential = true,
        ExcludeManagedIdentityCredential = true
    };
    
    DefaultAzureCredential credential = new DefaultAzureCredential(options);
    
    string blobServiceEndpoint = $"https://{azStorageAccount}.blob.core.windows.net";
    BlobServiceClient blobServiceClient = new BlobServiceClient(new Uri(blobServiceEndpoint), credential);

    var containerName = $"testcon{azStorageAccount}{rng}";
    var containerClient = (await blobServiceClient.CreateBlobContainerAsync(containerName)).Value;

    if (containerClient != null)
    {
        Console.WriteLine($"Container {containerName} created");
    }
    else
    {
        Console.WriteLine($"Container {containerName} failed to create");
    }

    var (filePath, fileName) = await CreateAMockFile(env, rng);
    
    BlobClient blobClient = containerClient.GetBlobClient(fileName);
    using (var fs = File.OpenRead(filePath))
    {
        await blobClient.UploadAsync(fs);
        fs.Close();
    }
    
    if (await blobClient.ExistsAsync())
    {
        Console.WriteLine("File uploaded successfully");
    }
    else
    {
        Console.WriteLine("File upload failed, exiting program..");
    }
}

static async Task<(string, string)> CreateAMockFile(string env = "undefined", int rng = 0)
{
    string localPath = "./";
    string fileName = "test" + env + rng.ToString() + ".txt";
    string localFilePath = Path.Combine(localPath, fileName);
    
    await File.WriteAllTextAsync(localFilePath, "Hello, World!");
    
    return (localFilePath, fileName);
}