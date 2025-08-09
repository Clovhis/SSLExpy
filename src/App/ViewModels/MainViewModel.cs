using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AzureKvSslExpirationChecker.Models;
using AzureKvSslExpirationChecker.Services;

namespace AzureKvSslExpirationChecker.ViewModels
{
    /// <summary>
    /// View model backing the main window. Handles user input and orchestrates scanning.
    /// </summary>
    public partial class MainViewModel : ObservableObject
    {
        private readonly AzureScanService _scanService;
        private readonly ReportWriter _reportWriter;

        public MainViewModel()
        {
            var auth = new AzureAuthFactory();
            _scanService = new AzureScanService(auth);
            _reportWriter = new ReportWriter();
            Records = new ObservableCollection<CertificateRecord>();
            Logs = new ObservableCollection<string>();
            ThresholdDays = 30;
        }

        [ObservableProperty]
        private string subscriptionId = string.Empty;

        [ObservableProperty]
        private string tenantId = string.Empty;

        [ObservableProperty]
        private string clientId = string.Empty;

        [ObservableProperty]
        private string clientSecret = string.Empty; // Never logged

        [ObservableProperty]
        private int thresholdDays;

        [ObservableProperty]
        private string outputFolder = string.Empty;

        [ObservableProperty]
        private bool isScanning;

        /// <summary>True when scanning is not in progress.</summary>
        public bool CanStart => !IsScanning;

        partial void OnIsScanningChanged(bool value) => OnPropertyChanged(nameof(CanStart));

        [ObservableProperty]
        private string progressText = string.Empty;

        [ObservableProperty]
        private string summaryText = string.Empty;

        public ObservableCollection<CertificateRecord> Records { get; }

        public ObservableCollection<string> Logs { get; }

        private CancellationTokenSource? _cts;

        /// <summary>Browse for an output folder.</summary>
        [RelayCommand]
        private void BrowseFolder()
        {
            using var dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                OutputFolder = dlg.SelectedPath;
            }
        }

        /// <summary>Open the output folder in Explorer.</summary>
        [RelayCommand]
        private void OpenOutputFolder()
        {
            if (Directory.Exists(OutputFolder))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = OutputFolder,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
        }

        /// <summary>Cancel an ongoing scan.</summary>
        [RelayCommand]
        private void Cancel()
        {
            _cts?.Cancel();
        }

        /// <summary>Start scanning the subscription for expiring certificates.</summary>
        [RelayCommand]
        private async Task StartScanAsync()
        {
            if (IsScanning)
                return;

            if (string.IsNullOrWhiteSpace(SubscriptionId) ||
                string.IsNullOrWhiteSpace(TenantId) ||
                string.IsNullOrWhiteSpace(ClientId) ||
                string.IsNullOrWhiteSpace(ClientSecret) ||
                string.IsNullOrWhiteSpace(OutputFolder) ||
                ThresholdDays < 1 || ThresholdDays > 365)
            {
                System.Windows.MessageBox.Show("Please fill in all fields with valid values.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsScanning = true;
            Records.Clear();
            Logs.Clear();
            ProgressText = string.Empty;
            SummaryText = string.Empty;
            _cts = new CancellationTokenSource();
            var progress = new Progress<string>(msg =>
            {
                Logs.Add(msg);
                ProgressText = msg;
            });

            try
            {
                var result = await _scanService.ScanAsync(
                    SubscriptionId,
                    TenantId,
                    ClientId,
                    ClientSecret,
                    ThresholdDays,
                    progress,
                    _cts.Token).ConfigureAwait(false);

                foreach (var r in result.Records)
                    Records.Add(r);

                var reportPath = await _reportWriter.WriteTxtAsync(result, SubscriptionId, ThresholdDays, OutputFolder, _cts.Token).ConfigureAwait(false);
                SummaryText = $"Vaults: {result.VaultCount}  Certificates: {result.CertificateCount}  Warnings: {result.WarningCount}  Duration: {result.Duration}";
                Logs.Add($"Report saved to {reportPath}");
            }
            catch (OperationCanceledException)
            {
                Logs.Add("Scan canceled.");
            }
            catch (Exception ex)
            {
                Logs.Add($"Error: {ex.Message}");
                System.Windows.MessageBox.Show("An error occurred. See log for details.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsScanning = false;
            }
        }
    }
}
