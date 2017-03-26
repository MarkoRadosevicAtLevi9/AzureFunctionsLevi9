Write-Output "Starting PS execution"


$Global:ErrorActionPreference = "Stop"

Write-Output "Setting strict mode"
Set-StrictMode -Version 2
try { cd (Split-Path -parent $PSCommandPath) } catch {}

$RootFolder = "D:\home\site\wwwroot\CreateVM"

$configFile = "$RootFolder\config.json"
Write-Output "Load configuration from $configFile"
$config = [string]::Join(' ', (Get-Content $configFile)) | ConvertFrom-Json

$secpasswd = ConvertTo-SecureString $config.Key -AsPlainText -Force;
$mycreds = New-Object System.Management.Automation.PSCredential ($config.ApplicationId, $secpasswd)

Write-Output "Connecting to Azure subscription"
Login-AzureRmAccount -ServicePrincipal -Tenant $config.TenantId -Credential $mycreds -SubscriptionName $config.AzureSubscriptionName
Write-Output "Connected!"
Get-AzureRmResourceGroup -Name LEVI9-resource-group | Remove-AzureRmResourceGroup -Verbose -Force
       
