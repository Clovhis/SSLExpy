using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AzureKvSslExpirationChecker.Models;

namespace AzureKvSslExpirationChecker.Services
{
    /// <summary>
    /// Persists scan results to a timestamped text file for offline review.
    /// </summary>
    public class ReportWriter
    {
        /// <summary>
        /// Writes the scan result to a text file and returns the file path.
        /// </summary>
        public async Task<string> WriteTxtAsync(ScanResult result, string subscriptionId, int thresholdDays, string outputFolder, CancellationToken ct)
        {
            Directory.CreateDirectory(outputFolder);
            var fileName = $"kv-ssl-scan_{DateTime.UtcNow:yyyy-MM-dd_HH-mm-ssZ}.txt";
            var path = Path.Combine(outputFolder, fileName);

            using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
            using var writer = new StreamWriter(stream, new UTF8Encoding(false));

            await writer.WriteLineAsync("Azure Key Vault SSL Certificate Scan").ConfigureAwait(false);
            await writer.WriteLineAsync($"Subscription: {subscriptionId}").ConfigureAwait(false);
            await writer.WriteLineAsync($"Threshold: {thresholdDays} days").ConfigureAwait(false);
            await writer.WriteLineAsync($"Timestamp (UTC): {result.ScannedAtUtc:O}").ConfigureAwait(false);
            await writer.WriteLineAsync(string.Empty).ConfigureAwait(false);

            string header = string.Format(
                "{0,-20} {1,-40} {2,-30} {3,8} {4,25} {5,25} {6,8} {7,8}",
                "Vault", "Certificate", "Version", "Enabled", "NotBefore", "ExpiresOn", "Days", "Warn");
            await writer.WriteLineAsync(header).ConfigureAwait(false);
            await writer.WriteLineAsync(new string('-', header.Length)).ConfigureAwait(false);

            foreach (var r in result.Records)
            {
                ct.ThrowIfCancellationRequested();
                string line = string.Format(
                    "{0,-20} {1,-40} {2,-30} {3,8} {4,25} {5,25} {6,8} {7,8}",
                    r.VaultName,
                    r.CertificateName,
                    r.Version ?? "-",
                    r.Enabled.HasValue ? r.Enabled.Value.ToString() : "-",
                    r.NotBefore?.ToString("u") ?? "N/A",
                    r.ExpiresOn?.ToString("u") ?? "N/A",
                    r.DaysUntilExpiry?.ToString() ?? "-",
                    r.IsWarning ? "Warning!" : string.Empty);
                await writer.WriteLineAsync(line).ConfigureAwait(false);
            }

            await writer.WriteLineAsync(string.Empty).ConfigureAwait(false);
            await writer.WriteLineAsync($"Vaults: {result.VaultCount}  Certificates: {result.CertificateCount}  Warnings: {result.WarningCount}").ConfigureAwait(false);
            await writer.WriteLineAsync($"Duration: {result.Duration}").ConfigureAwait(false);
            return path;
        }
    }
}
