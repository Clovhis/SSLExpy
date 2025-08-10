using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using AzureKvSslExpirationChecker.Services;

namespace AzureKvSslExpirationChecker
{
    /// <summary>
    /// Interaction logic for App.
    /// Initializes logging and global exception handlers.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Configure logging and exception handlers when the application starts.
        /// </summary>
        /// <param name="e">Startup event args.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Logger.Log("Application starting");

            // Global exception handlers
            DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        }

        private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Logger.Log("Unhandled UI exception", e.Exception);
            MessageBox.Show($"An unexpected error occurred. See log at {Logger.LogPath}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        private static void OnUnhandledException(object? sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                Logger.Log("Unhandled domain exception", ex);
            }
            else
            {
                Logger.Log($"Unhandled domain exception: {e.ExceptionObject}");
            }
        }

        private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            Logger.Log("Unobserved task exception", e.Exception);
            e.SetObserved();
        }
    }
}
