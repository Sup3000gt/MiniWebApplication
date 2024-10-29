using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using MiniWebApplication.Data;
using MiniWebApplication.Helpers;
using MiniWebApplication.Services;
using System.Globalization;

namespace MiniWebApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Set the default culture to en-US for USD formatting
            var usCulture = new CultureInfo("en-US");
            CultureInfo.DefaultThreadCurrentCulture = usCulture;
            CultureInfo.DefaultThreadCurrentUICulture = usCulture;

            // Read Cosmos DB settings from appsettings.json
            var cosmosDbSettings = builder.Configuration.GetSection("CosmosDb");

            var cosmosClient = new CosmosClient(
                cosmosDbSettings["AccountEndpoint"],
                cosmosDbSettings["AccountKey"]
            );

            // Register CosmosDbService with Dependency Injection
            builder.Services.AddSingleton(new CosmosDbService(
                cosmosClient,
                cosmosDbSettings["DatabaseName"],
                cosmosDbSettings["ContainerName"]
            ));

            // Add services to the container.
            builder.Services.AddLogging();
            builder.Services.AddControllersWithViews();
            builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            builder.Services.AddScoped<RazorViewToStringRenderer>();

            // Add DbContext
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Configure authentication
            builder.Services.AddAuthentication("CookieAuth")
                .AddCookie("CookieAuth", options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                });

            var app = builder.Build();

            // Seed the database
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    SeedData.Initialize(services);
                }
                catch (Exception ex)
                {
                    // Log any errors that occur during seeding
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred seeding the DB.");
                }
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.Use(async (context, next) =>
            {
                var logger = context.RequestServices.GetService<ILogger<Program>>();
                logger.LogInformation("Middleware executed for path: {Path}", context.Request.Path);
                await next();
            });


            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
