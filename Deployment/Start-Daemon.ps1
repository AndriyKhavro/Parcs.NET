param(
    [Parameter(Mandatory=$True)]
    [string] $BinFolder,
    [Parameter(Mandatory=$False)]
    [string] $LocalIp
)

$ServiceName = "ParcsDaemon"

Start-Process $BinFolder\DaemonPr.exe $LocalIp

#& sc.exe delete $ServiceName
#New-Service -Name $ServiceName -BinaryPathName "$BinFolder\DaemonPr.exe" -StartupType Manual

#if (!$LocalIp)
#{
 #   $LocalIp = ""
#}

#(new-Object System.ServiceProcess.ServiceController($ServiceName)).Start($LocalIp)
