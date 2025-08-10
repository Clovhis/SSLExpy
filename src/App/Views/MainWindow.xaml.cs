using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
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
            btnStartScan.Click += btnStartScan_Click;
            btnCancel.Click += btnCancel_Click;
            _scanService = new AzureScanService(new AzureAuthFactory());
        }

        private void AppendLog(string message)
        {
            string line = $"[{DateTime.UtcNow:HH:mm:ss}] {message}";
            if (rtbLog.Dispatcher.CheckAccess())
            {
                rtbLog.AppendText(line + Environment.NewLine);
                rtbLog.ScrollToEnd();
            }
            else
            {
                rtbLog.Dispatcher.BeginInvoke(new Action(() =>
                {
                    rtbLog.AppendText(line + Environment.NewLine);
                    rtbLog.ScrollToEnd();
                }));
            }
        }

        private async void btnStartScan_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnStartScan.IsEnabled = false;
                btnCancel.IsEnabled = true;
                var vm = (MainViewModel)DataContext;
                vm.Records.Clear();
                rtbLog.Document.Blocks.Clear();
                _cts = new CancellationTokenSource();

                string subscriptionId = vm.SubscriptionId?.Trim();
                string tenantId = vm.TenantId?.Trim();
                string clientId = vm.ClientId?.Trim();
                string clientSecret = vm.ClientSecret;
                string outputFolder = vm.OutputFolder?.Trim();
                int warningDays = vm.ThresholdDays;

                if (string.IsNullOrWhiteSpace(subscriptionId) ||
                    string.IsNullOrWhiteSpace(tenantId) ||
                    string.IsNullOrWhiteSpace(clientId) ||
                    string.IsNullOrWhiteSpace(clientSecret) ||
                    string.IsNullOrWhiteSpace(outputFolder))
                {
                    AppendLog("Faltan datos obligatorios (Subscription/Tenant/Client/Secret/Output).");
                    return;
                }

                Directory.CreateDirectory(outputFolder);

                AppendLog("Iniciando autenticación…");
                var cred = new ClientSecretCredential(tenantId, clientId, clientSecret);
                AppendLog("Autenticado OK.");

                var progress = new Progress<string>(msg => AppendLog(msg));

                await _scanService.ScanAsync(subscriptionId, tenantId, clientId, clientSecret, warningDays, progress, _cts.Token, record =>
                {
                    Dispatcher.BeginInvoke(new Action(() => vm.Records.Add(record)));
                });

                AppendLog("Scan finalizado.");
            }
            catch (RequestFailedException ex)
            {
                AppendLog($"Error Azure ({ex.Status}/{ex.ErrorCode}): {ex.Message}");
            }
            catch (OperationCanceledException)
            {
                AppendLog("Operación cancelada por el usuario.");
            }
            catch (Exception ex)
            {
                AppendLog($"Error inesperado: {ex.Message}");
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
            AppendLog("Cancelando…");
        }
    }
}
