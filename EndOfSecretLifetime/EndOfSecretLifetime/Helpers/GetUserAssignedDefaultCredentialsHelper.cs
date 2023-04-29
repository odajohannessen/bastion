using Azure.Identity;
using System;

namespace EndOfSecretLifetime.Helpers;

public class GetUserAssignedDefaultCredentialsHelper
{
    // Get User assigned DC
    public static DefaultAzureCredential GetUADC()
    {
        // Exclude options to decrease time spent trying to authenticate using each alternative
        string userAssignedClientId = Environment.GetEnvironmentVariable("UserAssignedClientId");
        var options = new DefaultAzureCredentialOptions
        {
            ExcludeEnvironmentCredential = true,
            ExcludeManagedIdentityCredential = false, // Set to false for deploy, true for local testing
            ExcludeSharedTokenCacheCredential = true,
            ExcludeVisualStudioCodeCredential = true,
            ExcludeVisualStudioCredential = true, // Set to true for deploy, false for local testing
            ExcludeAzureCliCredential = true,
            ExcludeAzurePowerShellCredential = true,
            ExcludeInteractiveBrowserCredential = true,
            ManagedIdentityClientId = userAssignedClientId, // Comment out during testing
        };
        var credentials = new DefaultAzureCredential(options);
        return credentials;
    }
}
