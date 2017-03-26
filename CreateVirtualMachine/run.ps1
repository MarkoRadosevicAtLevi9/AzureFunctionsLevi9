$in = Get-Content $triggerInput
Write-Output "Function processed queue message '$in'"

$inputParam = $in | ConvertFrom-Json

Write-Output "Starting PS execution"
Function GetRandomString(
    [int]$Length
)
{
	$set = "abcdefghijklmnopqrstuvwxyz0123456789".ToCharArray()
	$result = ""
	for ($x = 0; $x -lt $Length; $x++) {
		$result += $set | Get-Random
	}
	return $result
}

$Global:ErrorActionPreference = "Stop"

Write-Output "Setting strict mode"
Set-StrictMode -Version 2
try { cd (Split-Path -parent $PSCommandPath) } catch {}

$RootFolder = "D:\home\site\wwwroot\CreateVirtualMachine"

$configFile = "$RootFolder\config.json"
Write-Output "Load configuration from $configFile"
$config = [string]::Join(' ', (Get-Content $configFile)) | ConvertFrom-Json



Try
{
	$secpasswd = ConvertTo-SecureString $config.Key -AsPlainText -Force;
	$mycreds = New-Object System.Management.Automation.PSCredential ($config.ApplicationId, $secpasswd)

	Write-Output "Connecting to Azure subscription"
	Login-AzureRmAccount -ServicePrincipal -Tenant $config.TenantId -Credential $mycreds -SubscriptionName $config.AzureSubscriptionName
	Write-Output "Connected!"


	$loc = 'West Europe'

	$resourceGroupName = "LEVI9-resource-group"
	$vmName = $inputParam.VirtualMachineName
	$vmAdminUsername = $inputParam.UserName
	$vmAdminPassword = $inputParam.Password
	$subnetName = "LEVI9-subnet"
	$vNetName = "LEVI9-virtual-network"
	$secGroupRuleName = "LEVI9-security-group-rules"
	$secGroupName = "LEVI9-security-group"
	$networkInterfaceName = "LEVI9-network-interface"


	if ((Get-AzureRmResourceGroup $resourceGroupName -ErrorAction SilentlyContinue) -eq $null) {
		Write-Output "Creating resource group '$resourceGroupName'"
		New-AzureRmResourceGroup -Name $resourceGroupName -Location $loc

		# Create a subnet configuration
		$subnetConfig = New-AzureRmVirtualNetworkSubnetConfig -Name $subnetName -AddressPrefix 192.168.1.0/24

		# Create a virtual network
		$vnet = New-AzureRmVirtualNetwork -ResourceGroupName $resourceGroupName -Location $loc `
		-Name $vNetName -AddressPrefix 192.168.0.0/16 -Subnet $subnetConfig

		# Create a public IP address and specify a DNS name
		$pip = New-AzureRmPublicIpAddress -ResourceGroupName $resourceGroupName -Location $loc `
		-AllocationMethod Static -IdleTimeoutInMinutes 4 -Name "mypublicdns$(Get-Random)"

		# Create an inbound network security group rule for port 3389
		$nsgRuleRDP = New-AzureRmNetworkSecurityRuleConfig -Name $secGroupRuleName  -Protocol Tcp `
		-Direction Inbound -Priority 1000 -SourceAddressPrefix * -SourcePortRange * -DestinationAddressPrefix * `
		-DestinationPortRange 3389 -Access Allow

		# Create a network security group
		$nsg = New-AzureRmNetworkSecurityGroup -ResourceGroupName $resourceGroupName -Location $loc `
		-Name $secGroupName -SecurityRules $nsgRuleRDP

		# Create a virtual network card and associate with public IP address and NSG
		$nic = New-AzureRmNetworkInterface -Name $networkInterfaceName -ResourceGroupName $resourceGroupName -Location $loc `
		-SubnetId $vnet.Subnets[0].Id -PublicIpAddressId $pip.Id -NetworkSecurityGroupId $nsg.Id

		# Create a virtual machine configuration
		$vmsecpasswd = ConvertTo-SecureString $vmAdminPassword -AsPlainText -Force;
		$vmCreds = New-Object System.Management.Automation.PSCredential ($vmAdminUsername, $vmsecpasswd)

		$vmConfig = New-AzureRmVMConfig -VMName $vmName -VMSize Standard_DS1_V2 | `
		Set-AzureRmVMOperatingSystem -Windows -ComputerName $vmName -Credential $vmCreds  | `
		Set-AzureRmVMSourceImage -PublisherName MicrosoftWindowsServer -Offer WindowsServer -Skus 2016-Datacenter -Version latest | `
		Add-AzureRmVMNetworkInterface -Id $nic.Id
		$randomString = GetRandomString 4
		Set-AzureRmVMOSDisk -VM $vmConfig -Name "vmOSDisk123" -VhdUri "https://testwebtorage.blob.core.windows.net/vhds/vmOSDisk123.vhd" -CreateOption fromImage

		New-AzureRmVM -ResourceGroupName $resourceGroupName -Location $loc -VM $vmConfig

		$content = [string]::Format('{{ "Subject": "Virtual machine created", "Content": [ {{ "Type": "text/plain", "Value": "Virtual machine ({0}) has been created!" }} ] }}', $vmName)

		$content | Out-File -Encoding UTF8 $message

	}

}
Catch
{
	$errorMessage = $_.Exception.Message
	$fullError = $_.Exception.ToString()

	Write-Output "FAILED: $fullError"
	Write-Error $fullError
}