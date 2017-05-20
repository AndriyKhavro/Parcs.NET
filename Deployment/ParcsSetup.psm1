function Start-HostServer(
    [Parameter(Mandatory=$True)]
    [string] $BinFolder,
    [Parameter(Mandatory=$False)]
    [string[]] $IpAddresses,
    [Parameter(Mandatory=$False)]
    [string] $LocalIp
)
{
	if ($IpAddresses)
	{
		$HostFileContent = $IpAddresses -join "`r`n" | Out-String
		New-Item "$BinFolder\hosts.txt" -ItemType File -Force -Value $HostFileContent
	}

    Start-Process $BinFolder\HostServer.exe $LocalIp
}

function Start-Daemon(
    [Parameter(Mandatory=$True)]
    [string] $BinFolder,
    [Parameter(Mandatory=$False)]
    [string] $LocalIp
)
{
    Start-Process $BinFolder\DaemonPr.exe $LocalIp
}

function Install-AndRunService(
    [Parameter(Mandatory=$True)]
    [string] $ServiceName,
    [Parameter(Mandatory=$True)]
    [string] $BinaryPath,
    [Parameter(Mandatory=$False)]
    [string] $StartParameters
)
{
    & sc.exe delete $ServiceName
    New-Service -Name $ServiceName -BinaryPathName "$BinaryPath" -StartupType Manual

    if (!$StartParameters)
    {
       $StartParameters = ""
    }

    (new-Object System.ServiceProcess.ServiceController($ServiceName)).Start($LocalIp)
}

Export-ModuleMember -Function "Start-Daemon"
Export-ModuleMember -Function "Start-HostServer"