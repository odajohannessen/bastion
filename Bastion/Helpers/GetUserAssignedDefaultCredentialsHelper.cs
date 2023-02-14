using Azure.Identity;

namespace Bastion.Helpers;

public class GetUserAssignedDefaultCredentialsHelper
{
    // Get User assigned DC
    public static DefaultAzureCredential GetUADC()
    {
        // Exclude options to decrease time spent checking options
        string userAssignedClientId = "eafbb947-013f-43d6-8c3c-9ab9ef3e1e4e";
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
