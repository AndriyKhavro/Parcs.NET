FROM microsoft/windowsservercore

WORKDIR /parcs-hostserver

ADD bin/Release /parcs-hostserver

EXPOSE 1234

EXPOSE 1236

CMD ["HostServer.exe", "--docker"]