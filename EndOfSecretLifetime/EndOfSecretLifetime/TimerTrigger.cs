using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using EndOfSecretLifetime.Managers;
using EndOfSecretLifetime.Helpers;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EndOfSecretLifetime;

public class TimerTrigger
{
    private readonly StorageManager storageManager;
    private readonly LoggingManager logging;

    public TimerTrigger(StorageManager _storageManager, LoggingManager _logging)
    {
        storageManager = _storageManager;
        logging = _logging;
    }

    [FunctionName("TimerTrigger")]
    public async Task Run([TimerTrigger("0 */5 * * * *"
    #if DEBUG
        , RunOnStartup=true
    #endif
    )] TimerInfo myTimer)
    {
        logging.LogEvent("Starting life time check of stored secrets");

        string storageContainerName = Environment.GetEnvironmentVariable("StorageContainerName");
        bool success = await storageManager.CheckExpirationAndDelete(storageContainerName);

        logging.LogEvent("Finished life time check of stored secrets");        
    }
}
