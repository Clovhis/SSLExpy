using System;

namespace AzureKvSslExpirationChecker.Models
{
    /// <summary>
    /// Represents a single certificate entry returned from Azure Key Vault.
    /// </summary>
    public class CertificateRecord
    {
        /// <summary>Name of the Key Vault containing the certificate.</summary>
        public string VaultName { get; set; } = string.Empty;

        /// <summary>URI of the Key Vault.</summary>
        public string VaultUri { get; set; } = string.Empty;

        /// <summary>Name of the certificate.</summary>
        public string CertificateName { get; set; } = string.Empty;

        /// <summary>Specific certificate version identifier.</summary>
        public string? Version { get; set; }

        /// <summary>Indicates whether the certificate is enabled.</summary>
        public bool? Enabled { get; set; }

        /// <summary>Start of the certificate validity period.</summary>
        public DateTimeOffset? NotBefore { get; set; }

        /// <summary>End of the certificate validity period.</summary>
        public DateTimeOffset? ExpiresOn { get; set; }

        /// <summary>Number of days remaining until expiration. Null if expiration is not set.</summary>
        public int? DaysUntilExpiry { get; set; }

        /// <summary>True when the certificate has already expired.</summary>
        public bool IsExpired => DaysUntilExpiry < 0;

        /// <summary>True when the certificate is at or below the warning threshold.</summary>
        public bool IsWarning { get; set; }
    }
}
