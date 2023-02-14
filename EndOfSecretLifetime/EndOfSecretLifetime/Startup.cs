using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using EndOfSecretLifetime.Managers;

[assembly: FunctionsStartup(typeof(EndOfSecretLifetime.Startup))]
namespace EndOfSecretLifetime
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddScoped<LoggingManager>();
            builder.Services.AddScoped<StorageManager>();
        }
    }
}