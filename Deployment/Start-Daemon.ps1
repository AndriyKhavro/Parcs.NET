param(
    [Parameter(Mandatory=$True)]
    [string] $BinFolder,
    [Parameter(Mandatory=$False)]
    [string] $LocalIp,
	[Parameter(Mandatory=$False)]
    [string] $SetupAsWindowsService = $False
)

if ($SetupAsWindowsService)
{
	$ServiceName = "ParcsDaemon"
	& sc.exe delete $ServiceName
	New-Service -Name $ServiceName -BinaryPathName "$BinFolder\DaemonPr.exe" -StartupType Manual

	if (!$LocalIp)
	{
		$LocalIp = ""
	}

	(new-Object System.ServiceProcess.ServiceController($ServiceName)).Start($LocalIp)
}



Start-Process $BinFolder\DaemonPr.exe $LocalIp


