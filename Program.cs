using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;
using MiniWebApplication.Services;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MiniWebApplication.Data;
using MiniWebApplication.Helpers;

namespace MiniWebApplication
{
    public class Program
    {
        public static async Task Main(string[] args) // Make Main async to allow warm-up
        {
            var builder = WebApplication.CreateBuilder(args);

            // Set the default culture to en-US for USD formatting
            var usCulture = new CultureInfo("en-US");
            CultureInfo.DefaultThreadCurrentCulture = usCulture;
            CultureInfo.DefaultThreadCurrentUICulture = usCulture;

            // Configure Logging
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();

            // Read Cosmos DB settings from appsettings.json
            var cosmosDbSettings = builder.Configuration.GetSection("CosmosDb");

            // Register CosmosClient as a singleton
            builder.Services.AddSingleton<CosmosClient>(serviceProvider =>
            {
                string accountEndpoint = cosmosDbSettings["AccountEndpoint"];
                string accountKey = cosmosDbSettings["AccountKey"];

                if (string.IsNullOrEmpty(accountEndpoint) || string.IsNullOrEmpty(accountKey))
                {
                    throw new InvalidOperationException("Cosmos DB AccountEndpoint and AccountKey must be provided in configuration.");
                }

                var cosmosClientOptions = new CosmosClientOptions
                {
                    ConnectionMode = ConnectionMode.Direct,
                    MaxRetryAttemptsOnRateLimitedRequests = 3,
                    MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(5),
                    OpenTcpConnectionTimeout = TimeSpan.FromSeconds(5),  // Reduce open timeout
                    IdleTcpConnectionTimeout = TimeSpan.FromMinutes(15), // Keep connections alive longer
                    MaxRequestsPerTcpConnection = 50,  // Increase requests per connection
                    MaxTcpConnectionsPerEndpoint = 10  // Increase max concurrent connections
                };

                return new CosmosClient(accountEndpoint, accountKey, cosmosClientOptions);
            });

            builder.Services.AddSingleton<CosmosDbService>(serviceProvider =>
            {
                var cosmosClient = serviceProvider.GetRequiredService<CosmosClient>();
                var logger = serviceProvider.GetRequiredService<ILogger<CosmosDbService>>();

                string databaseName = cosmosDbSettings["DatabaseName"];
                string containerName = cosmosDbSettings["ContainerName"];

                if (string.IsNullOrEmpty(databaseName) || string.IsNullOrEmpty(containerName))
                {
                    throw new InvalidOperationException("Cosmos DB DatabaseName and ContainerName must be provided in configuration.");
                }

                return new CosmosDbService(cosmosClient, databaseName, containerName, logger);
            });

            // Add Data Protection Service for secure data handling
            builder.Services.AddDataProtection();
            // Add other necessary services
            builder.Services.AddLogging();
            builder.Services.AddControllersWithViews();
            builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            builder.Services.AddScoped<RazorViewToStringRenderer>();

            // Add DbContext for Entity Framework Core
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Configure authentication using cookies
            builder.Services.AddAuthentication("CookieAuth")
                .AddCookie("CookieAuth", options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                });

            var app = builder.Build();

            // Warm-up CosmosClient on application startup
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var cosmosClient = services.GetRequiredService<CosmosClient>();

                    // Reuse the existing cosmosDbSettings from the outer scope
                    string databaseName = cosmosDbSettings["DatabaseName"];
                    string containerName = cosmosDbSettings["ContainerName"];

                    var container = cosmosClient.GetContainer(databaseName, containerName);

                    // Perform a dummy query to warm up the client
                    await container.ReadContainerAsync();

                    // Seed the database
                    SeedData.Initialize(services);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred during Cosmos DB warm-up or DB seeding.");
                }
            }


            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();


            // Custom middleware for logging request paths
            app.Use(async (context, next) =>
            {
                var logger = context.RequestServices.GetService<ILogger<Program>>();

                // Log the incoming request path
                logger.LogInformation("Middleware executed for path: {Path}", context.Request.Path);
                Console.WriteLine($"Request: {context.Request.Path}");

                // Call the next middleware in the pipeline
                await next.Invoke();

                // Log the response status code after the request is processed
                Console.WriteLine($"Response: {context.Response.StatusCode}");
            });


            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            await app.RunAsync(); // Use RunAsync to support async Main
        }
    }
}
