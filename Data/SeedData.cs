using Microsoft.Extensions.DependencyInjection;
using MiniWebApplication.Data;
using MiniWebApplication.Models;
using System;
using System.Linq;

namespace MiniWebApplication.Data 
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            if (!context.Products.Any())
            {
                context.Products.AddRange(
                    new Product
                    {
                        Name = "Product 1",
                        Description = "Description of Product 1",
                        Price = 10.99m,
                        ImageUrl = "/images/products/product1.png"
                    }
                );

                context.SaveChanges();
            }


            context.SaveChanges();
        }
    }
}

