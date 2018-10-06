$ErrorActionPreference="Stop"

docker run -d --name=hostserver --rm andriikhavro/parcshostserver:windowsservercore-1709

$HostServerIp = (docker inspect -f '{{.NetworkSettings.Networks.nat.IPAddress}}' hostserver) | Out-String

Write-Host "Using Host Server IP: $HostServerIp"

docker run -d --name=daemon1 --rm -e PARCS_HOST_SERVER_IP_ADDRESS=$HostServerIp andriikhavro/parcsdaemon:windowsservercore-1709