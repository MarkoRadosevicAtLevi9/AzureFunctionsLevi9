$RootFolder = "D:\home\site\wwwroot\TimerTriggerPowerShell2"

$configFile = "$RootFolder\config.json"
Write-Output "Load configuration from $configFile"
$config = [string]::Join(' ', (Get-Content $configFile)) | ConvertFrom-Json

$resourceGroupName = "LEVI9-resource-group"

Try
{
	$secpasswd = ConvertTo-SecureString $config.Key -AsPlainText -Force;
	$mycreds = New-Object System.Management.Automation.PSCredential ($config.ApplicationId, $secpasswd)

	Write-Output "Connecting to Azure subscription"
	Login-AzureRmAccount -ServicePrincipal -Tenant $config.TenantId -Credential $mycreds -SubscriptionName $config.AzureSubscriptionName
	Write-Output "Connected!"

	$VMs= Get-AzureRmVM -ResourceGroupName $resourceGroupName | where {$_.status -ne 'StoppedDealocated'}
	foreach ($vm in $VMs){
		Write-Output $vm.Name
		$stopRes = Stop-AzureRmVM -ResourceGroupName $resourceGroupName -Name $vm.Name
		Write-Output "$($vm.Name) virtual machine stopped."
	}
    
}Catch{
	$errorMessage = $_.Exception.Message
	$fullError = $_.Exception.ToString()

	Write-Output "FAILED: $fullError"
	Write-Error $fullError
}