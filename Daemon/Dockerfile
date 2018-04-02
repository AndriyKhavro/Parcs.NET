FROM microsoft/windowsservercore:1709

WORKDIR /parcs-daemon

EXPOSE 2222

ADD bin/Release /parcs-daemon

CMD ["DaemonPr.exe", "--docker"]