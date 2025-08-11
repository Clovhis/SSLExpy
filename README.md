# EY-GDS SRE SSL-ExpiryShield

A Windows desktop utility to authenticate with Azure using a Service Principal, enumerate all Key Vaults in a subscription, and export SSL/TLS certificate expirations with live logging and CSV/TXT reports.

- Highlights:
  - Live log panel with real-time progress
  - Enumerates Key Vaults via ARM; lists certificates via Key Vault data plane
  - Warning threshold configurable (default 90 days)
  - TXT and CSV export to the chosen Output Folder
  - Persistent user settings for all input fields
  - Color-coded rows: Red (expired), Yellow (<= 90 days), White (> 90 days)

## Getting Started
1. Prerequisites: .NET 8 Desktop Runtime, Azure credentials (Tenant ID, Client ID, Client Secret), Subscription ID, Key Vault data-plane permissions to list/read certificates.
2. Download the latest release (EY-GDS SRE SSL-ExpiryShield.zip), extract and run the executable.
3. Enter Tenant ID, Client ID, Client Secret, Subscription ID, Output Folder, and Warning Days.
4. Click "Start Scan" to authenticate, enumerate vaults, and generate the report. Use "Open Output Folder" to quickly access exports.

## Output
- A TXT file (certificates_yyyyMMdd_HHmmss.txt) and CSV file in the selected Output Folder with columns:
  Vault, Certificate, Version, Enabled, NotBefore, ExpiresOn, Days, Warning

## Permissions
- Control plane (enumerate Key Vaults): subscription-level Reader/Contributor/Owner etc.
- Data plane (list/read certificates): Key Vault RBAC roles (e.g., Key Vault Certificate User/Officer/Admin) or Access Policies that include certificates/list and certificates/get.

## Troubleshooting
- 401/403 when listing certificates: missing data-plane permissions on the vault.
- Empty results: ensure the subscription and tenant are correct and the SPN has access.

## Author & Ownership
- Author: [Leo Vargas](mailto:leonardo.r.vargas@gds.ey.com?subject=EY-GDS%20SRE%20SSL-ExpiryShield%20-%20Bug%20Report&body=Please%20include%20version%20and%20reproduction%20steps.)
- Property of EY GDS CTP-SRE Team

## License / Notice
- Internal use only (EY GDS). Do not redistribute without permission.

