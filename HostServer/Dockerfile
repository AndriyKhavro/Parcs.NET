FROM microsoft/windowsservercore:1709

WORKDIR /parcs-hostserver

EXPOSE 1234

EXPOSE 1236

ADD bin/Release /parcs-hostserver

CMD ["HostServer.exe", "--docker"]