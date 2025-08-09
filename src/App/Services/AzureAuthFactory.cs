using Azure.Identity;
using Azure.ResourceManager;

namespace AzureKvSslExpirationChecker.Services
{
    /// <summary>
    /// Creates authenticated Azure SDK clients using service principal credentials.
    /// </summary>
    public class AzureAuthFactory
    {
        /// <summary>
        /// Builds a <see cref="ClientSecretCredential"/> for the supplied service principal.
        /// </summary>
        public ClientSecretCredential CreateCredential(string tenantId, string clientId, string clientSecret)
            => new ClientSecretCredential(tenantId, clientId, clientSecret);

        /// <summary>
        /// Builds an <see cref="ArmClient"/> using the provided credential.
        /// </summary>
        public ArmClient CreateArmClient(ClientSecretCredential credential)
            => new ArmClient(credential);
    }
}
