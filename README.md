# Azure KV SSL Expiration Checker

## Overview
Azure KV SSL Expiration Checker is a portable Windows desktop application built with WPF and .NET 8. It authenticates with an Azure Service Principal and scans all Azure Key Vaults in a subscription for certificate expirations. Certificates nearing expiration are highlighted and included in a timestamped text report.

## Features
- Authenticate using Azure Client Secret credentials.
- Enumerate all Key Vaults and certificates in a subscription.
- Calculate days until each certificate expires.
- Highlight certificates that are near expiration.
- Export scan results to a UTF-8 text file.
- Responsive UI with progress logging and cancellation support.

## Prerequisites
- Windows 11.
- Azure subscription with Key Vaults.
- Service Principal with access to the subscription and **get/list** permissions for Key Vault certificates.

## Usage
1. Launch the provided `AzureKvSslExpirationChecker.exe`.
2. Enter the Subscription ID, Tenant ID, Client ID, and Client Secret.
3. Choose a warning threshold (days) and an output folder.
4. Click **Start Scan**.
5. Review results in the UI and open the generated report.

## Configuration Fields
| Field | Description |
|-------|-------------|
| Subscription ID | Azure subscription to scan. |
| Tenant ID | Azure Active Directory tenant. |
| Client ID | Application (client) ID of the Service Principal. |
| Client Secret | Secret for the Service Principal. |
| Warning threshold | Number of days before expiration that triggers a warning. |
| Output folder | Folder where the report will be saved. |

## Report Format
```
Azure Key Vault SSL Certificate Scan
Subscription: 00000000-0000-0000-0000-000000000000
Threshold: 30 days
Timestamp (UTC): 2024-01-01T00:00:00Z

Vault                Certificate                             Version                        Enabled NotBefore                 ExpiresOn                 Days Warn
-------------------- --------------------------------------- ------------------------------ ------- ------------------------- ------------------------- ---- ----
my-vault             www.contoso.com                         1234567890abcdef               True    2023-01-01 00:00:00Z      2024-01-01 00:00:00Z      10   Warning!
```

## Security Notes
- Secrets are never written to logs or files.
- Reports contain no credentials and can be safely archived.

## Troubleshooting
- **RBAC errors**: ensure the Service Principal has Key Vault `get/list` permissions.
- **Throttling**: the app retries transient failures automatically.
- **No Key Vaults found**: verify the subscription ID and permissions.

## Build
```bash
dotnet restore ./src/App/AzureKvSslExpirationChecker.csproj
dotnet build ./src/App/AzureKvSslExpirationChecker.csproj
```
To publish a portable folder:
```bash
dotnet publish ./src/App/AzureKvSslExpirationChecker.csproj -c Release -r win-x64 --self-contained true
```
Rename the publish output folder as desired and zip it (e.g., `AzureKvSslExpirationChecker_win-x64.zip`).

## CI Workflow
Pushes to branches matching `feature/*` trigger a GitHub Action that builds the project, publishes a self-contained folder, and creates a GitHub Release with the zipped artifact.

## License
MIT
