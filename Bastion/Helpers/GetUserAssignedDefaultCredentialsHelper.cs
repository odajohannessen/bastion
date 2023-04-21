using Azure.Identity;

namespace Bastion.Helpers;

public class GetUserAssignedDefaultCredentialsHelper
{
    // Get User assigned DC
    public static DefaultAzureCredential GetUADC()
    {
        // Exclude options to decrease time spent checking options
        string userAssignedClientId = "7a5e9bc8-041a-48a4-90cd-28b2b7539a45";
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
            ManagedIdentityClientId = userAssignedClientId, 
        };
        var credentials = new DefaultAzureCredential(options);
        return credentials;
    }
}
