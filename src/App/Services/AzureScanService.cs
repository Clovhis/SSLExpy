using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.KeyVault;
using Azure.ResourceManager.Resources;
using Azure.Security.KeyVault.Certificates;
using AzureKvSslExpirationChecker.Models;

namespace AzureKvSslExpirationChecker.Services
{
    /// <summary>
    /// Performs the heavy lifting of enumerating Key Vaults and certificates.
    /// </summary>
    public class AzureScanService
    {
        private readonly AzureAuthFactory _authFactory;

        public AzureScanService(AzureAuthFactory authFactory)
        {
            _authFactory = authFactory;
        }

        /// <summary>
        /// Executes the scan and returns aggregated results.
        /// </summary>
        public async Task<ScanResult> ScanAsync(
            string subscriptionId,
            string tenantId,
            string clientId,
            string clientSecret,
            int thresholdDays,
            IProgress<string> log,
            CancellationToken ct,
            Action<CertificateRecord>? onRecord = null)
        {
            var sw = Stopwatch.StartNew();
            var records = new List<CertificateRecord>();

            var credential = _authFactory.CreateCredential(tenantId, clientId, clientSecret);
            log.Report("Authenticating with Azure...");
            var arm = _authFactory.CreateArmClient(credential);
            var subscription = arm.GetSubscriptionResource(new ResourceIdentifier($"/subscriptions/{subscriptionId}"));

            var vaults = new List<KeyVaultResource>();
            log.Report("Enumerando vaults...");
            await RetryPolicy.RunAsync(async () =>
            {
                await foreach (var v in subscription.GetKeyVaultsAsync(cancellationToken: ct))
                {
                    vaults.Add(v);
                }
            }, log, ct);

            foreach (var vault in vaults)
            {
                ct.ThrowIfCancellationRequested();
                log.Report($"Procesando {vault.Data.Properties.VaultUri}...");
                var certClient = new CertificateClient(vault.Data.Properties.VaultUri!, credential);
                var before = records.Count;
                await RetryPolicy.RunAsync(async () =>
                {
                    await foreach (var prop in certClient.GetPropertiesOfCertificatesAsync(cancellationToken: ct))
                    {
                        var days = prop.ExpiresOn.HasValue ? (int)(prop.ExpiresOn.Value - DateTimeOffset.UtcNow).TotalDays : (int?)null;
                        var record = new CertificateRecord
                        {
                            VaultName = vault.Data.Name,
                            VaultUri = vault.Data.Properties.VaultUri!.ToString(),
                            CertificateName = prop.Name,
                            Version = prop.Version,
                            Enabled = prop.Enabled,
                            NotBefore = prop.NotBefore,
                            ExpiresOn = prop.ExpiresOn,
                            DaysUntilExpiry = days,
                            IsWarning = days.HasValue && days.Value <= thresholdDays
                        };
                        records.Add(record);
                        onRecord?.Invoke(record);
                    }
                }, log, ct);
                var found = records.Count - before;
                log.Report($"{found} certificados encontrados...");
            }

            sw.Stop();
            var result = new ScanResult
            {
                Records = records,
                VaultCount = vaults.Count,
                CertificateCount = records.Count,
                WarningCount = records.FindAll(r => r.IsWarning).Count,
                Duration = sw.Elapsed,
                ScannedAtUtc = DateTimeOffset.UtcNow
            };
            return result;
        }
    }
}
