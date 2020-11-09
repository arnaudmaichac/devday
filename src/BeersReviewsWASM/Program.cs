using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using BeersReviewWASM.Services;

namespace BeersReviewsWASM
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            ConfigureServices(builder.Services);

            builder.RootComponents.Add<App>("app");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            await builder.Build().RunAsync();
        }

        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ReviewProvider>();
            //services.AddSingleton(provider =>
            //{
            //    var config = provider.GetService<IConfiguration>();
            //    return config.GetSection("App").Get<AppConfiguration>();
            //});
        }

    }

    internal class AppConfiguration
    {
        public string tableName { get; set; }
        public string containerName { get; set; }
        public string queueName { get; set; }
        public string storageAccountConnectionString { get; set; }
    }
}
