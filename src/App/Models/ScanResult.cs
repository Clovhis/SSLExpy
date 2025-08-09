using System;
using System.Collections.Generic;

namespace AzureKvSslExpirationChecker.Models
{
    /// <summary>
    /// Holds aggregated results of a full scan across all Key Vaults.
    /// </summary>
    public class ScanResult
    {
        /// <summary>Collection of discovered certificate records.</summary>
        public IReadOnlyList<CertificateRecord> Records { get; set; } = Array.Empty<CertificateRecord>();

        /// <summary>Total number of vaults scanned.</summary>
        public int VaultCount { get; set; }

        /// <summary>Total number of certificates inspected.</summary>
        public int CertificateCount { get; set; }

        /// <summary>Total number of certificates flagged with warnings.</summary>
        public int WarningCount { get; set; }

        /// <summary>Duration of the scan.</summary>
        public TimeSpan Duration { get; set; }

        /// <summary>Timestamp when the scan completed (UTC).</summary>
        public DateTimeOffset ScannedAtUtc { get; set; }
    }
}
