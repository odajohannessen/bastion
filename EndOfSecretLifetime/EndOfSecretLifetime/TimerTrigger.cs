//using System;
//using Microsoft.Azure.WebJobs;
//using Microsoft.Azure.WebJobs.Host;
//using Microsoft.Extensions.Logging;
//using EndOfSecretLifetime.Managers;
//using EndOfSecretLifetime.Helpers;

//namespace EndOfSecretLifetime
//{
//    public class TimerTrigger
//    {
//        [FunctionName("Function1")]
//        public void Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer)
//        {
//            //log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
//            // TODO: Add func app to MI 

//            // Retrieve a list of the secrets currently in storage
//            string storageContainerName = "secrets-test";
//            var blobList = StorageManager.GetBlobNames(storageContainerName);


//            // What should the timer trigger do? 
//            // 1. Get list of names of blobs - important that the files won't be downloaded, only names
//            // 2. Innebygd sjekk om noen har levd mer enn 24 t
//            // Sett grense på 24 t
//            // List -> dict(key, dato) fra navn
//            // ISO8601 for date
//        }
//    }
//}
