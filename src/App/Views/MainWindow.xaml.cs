using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.ComponentModel;
using Azure;
using Azure.Identity;
using AzureKvSslExpirationChecker.Models;
using AzureKvSslExpirationChecker.Services;
using AzureKvSslExpirationChecker.ViewModels;

namespace AzureKvSslExpirationChecker.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CancellationTokenSource? _cts;
        private readonly AzureScanService _scanService;

        public MainWindow()
        {
            InitializeComponent();
            TxtSubscriptionId.Text = Properties.Settings.Default.SubscriptionId ?? string.Empty;
            TxtTenantId.Text       = Properties.Settings.Default.TenantId ?? string.Empty;
            TxtClientId.Text       = Properties.Settings.Default.ClientId ?? string.Empty;
            TxtOutputFolder.Text   = Properties.Settings.Default.OutputFolder ?? string.Empty;
            if (!string.IsNullOrEmpty(Properties.Settings.Default.ClientSecret))
                PwdClientSecret.Password = Properties.Settings.Default.ClientSecret;
            btnStartScan.Click += btnStartScan_Click;
            btnCancel.Click += btnCancel_Click;
            _scanService = new AzureScanService(new AzureAuthFactory());
        }

        private void Log(string message)
        {
            var line = $"[{DateTime.Now:HH:mm:ss}] {message}";
            if (TxtLog.Dispatcher.CheckAccess())
            {
                TxtLog.AppendText(line + Environment.NewLine);
                TxtLog.ScrollToEnd();
            }
            else
            {
                TxtLog.Dispatcher.BeginInvoke(new Action(() =>
                {
                    TxtLog.AppendText(line + Environment.NewLine);
                    TxtLog.ScrollToEnd();
                }));
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            Properties.Settings.Default.SubscriptionId = TxtSubscriptionId.Text.Trim();
            Properties.Settings.Default.TenantId       = TxtTenantId.Text.Trim();
            Properties.Settings.Default.ClientId       = TxtClientId.Text.Trim();
            Properties.Settings.Default.OutputFolder   = TxtOutputFolder.Text.Trim();
            // Properties.Settings.Default.ClientSecret = PwdClientSecret.Password; // only if chosen
            Properties.Settings.Default.Save();
        }

        private async void btnStartScan_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnStartScan.IsEnabled = false;
                btnCancel.IsEnabled = true;
                var vm = (MainViewModel)DataContext;
                vm.Records.Clear();
                TxtLog.Clear();
                _cts = new CancellationTokenSource();

                string subscriptionId = vm.SubscriptionId?.Trim();
                string tenantId = vm.TenantId?.Trim();
                string clientId = vm.ClientId?.Trim();
                string clientSecret = vm.ClientSecret;
                string outputFolder = vm.OutputFolder?.Trim();
                const int warningDays = 90;

                if (string.IsNullOrWhiteSpace(subscriptionId) ||
                    string.IsNullOrWhiteSpace(tenantId) ||
                    string.IsNullOrWhiteSpace(clientId) ||
                    string.IsNullOrWhiteSpace(clientSecret) ||
                    string.IsNullOrWhiteSpace(outputFolder))
                {
                    Log("Missing required data (Subscription/Tenant/Client/Secret/Output).");
                    return;
                }

                Directory.CreateDirectory(outputFolder);

                Log("Starting authentication...");
                var cred = new ClientSecretCredential(tenantId, clientId, clientSecret);
                Log("Authentication succeeded.");

                var progress = new Progress<string>(msg => Log(msg));

                await _scanService.ScanAsync(subscriptionId, tenantId, clientId, clientSecret, outputFolder, warningDays, progress, _cts.Token, record =>
                {
                    Dispatcher.BeginInvoke(new Action(() => vm.Records.Add(record)));
                });

                Log("Scan completed.");
            }
            catch (RequestFailedException ex)
            {
                Log($"Azure error ({ex.Status}/{ex.ErrorCode}): {ex.Message}");
            }
            catch (OperationCanceledException)
            {
                Log("Operation canceled by user.");
            }
            catch (Exception ex)
            {
                Log($"Unexpected error: {ex.Message}");
            }
            finally
            {
                btnStartScan.IsEnabled = true;
                btnCancel.IsEnabled = false;
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            _cts?.Cancel();
            Log("Canceling...");
        }
    }
}
