using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure;

namespace Bastion.Helpers;

public class GetSecretFromKeyVaultHelper
{
    // Access key vault and retrieve a secret
    public static string GetSecret(string secretName)
    {
        string keyVaultName = "kvbastion";
        var uri = $"https://{keyVaultName}.vault.azure.net/";
        var credentials = GetUserAssignedDefaultCredentialsHelper.GetUADC();
        SecretClient client = new SecretClient(new Uri(uri), credentials);
        Response<KeyVaultSecret> secret = client.GetSecret(secretName);

        return secret.Value.Value.ToString();
    }
}
