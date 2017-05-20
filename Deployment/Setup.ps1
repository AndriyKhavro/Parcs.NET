#Set-ExecutionPolicy Unrestricted

Import-Module -Name ".\Invoke-MsBuild.psm1"
Import-Module -Name ".\ParcsSetup.psm1"

#$VerbosePreference = "Continue"

Remove-Item "C:\Parcs.NET" -Recurse -Force

.\Copy-GithubRepository.ps1 -Name "Parcs.NET" -Author AndriyKhavro -OutVariable SolutionPath

& nuget restore $SolutionPath

Invoke-MsBuild -Path "$SolutionPath\Parcs.sln" -MsBuildParameters "/p:Configuration=Release" -ShowBuildOutputInNewWindow

Start-Daemon -BinFolder "$SolutionPath\Daemon\bin\Release" -LocalIp "192.168.11.112"

Start-HostServer -BinFolder "$SolutionPath\HostServer\bin\Release" -LocalIp "192.168.11.112" -IpAddresses @("192.168.11.112")

#.\Create-ParcsApiWebsite.ps1 -SiteDirectory "$SolutionPath\RestApi"