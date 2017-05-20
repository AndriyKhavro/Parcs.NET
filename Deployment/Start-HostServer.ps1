param(
    [Parameter(Mandatory=$True)]
    [string] $BinFolder,
    [Parameter(Mandatory=$False)]
    [string[]] $IpAddresses,
    [Parameter(Mandatory=$False)]
    [string] $LocalIp
)

#$ServiceName = "ParcsHostServer"

$HostFileContent = $IpAddresses -join "`r`n" | Out-String

if ($HostFileContent) 
{
	New-Item "$BinFolder\hosts.txt" -ItemType File -Force -Value $HostFileContent	
}

Start-Process $BinFolder\HostServer.exe $LocalIp

#& sc.exe delete $ServiceName
#New-Service -Name $ServiceName -BinaryPathName "$BinFolder\HostServer.exe $LocalIp" -StartupType Automatic
#Start-Service -Name $ServiceName

