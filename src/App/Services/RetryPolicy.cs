using System;
using System.Threading;
using System.Threading.Tasks;
using Azure;

namespace AzureKvSslExpirationChecker.Services
{
    /// <summary>
    /// Provides a lightweight retry helper with exponential backoff for transient Azure errors.
    /// </summary>
    public static class RetryPolicy
    {
        private const int MaxRetries = 3;
        private static readonly TimeSpan BaseDelay = TimeSpan.FromMilliseconds(500);

        /// <summary>
        /// Executes an asynchronous function with retry logic.
        /// </summary>
        public static async Task<T> RunAsync<T>(Func<Task<T>> action, IProgress<string>? log, CancellationToken ct)
        {
            for (int attempt = 1; ; attempt++)
            {
                ct.ThrowIfCancellationRequested();
                try
                {
                    return await action().ConfigureAwait(false);
                }
                catch (RequestFailedException ex) when (attempt < MaxRetries)
                {
                    log?.Report($"Transient error: {ex.Message}. Retrying...");
                    await Task.Delay(BaseDelay * Math.Pow(2, attempt - 1), ct).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Executes a non-returning asynchronous function with retry logic.
        /// </summary>
        public static async Task RunAsync(Func<Task> action, IProgress<string>? log, CancellationToken ct)
        {
            await RunAsync(async () =>
            {
                await action().ConfigureAwait(false);
                return true;
            }, log, ct).ConfigureAwait(false);
        }
    }
}
