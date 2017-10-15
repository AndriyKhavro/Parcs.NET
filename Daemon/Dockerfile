FROM microsoft/windowsservercore

WORKDIR /parcs-daemon

ADD bin/Release /parcs-daemon

EXPOSE 2222

CMD ["DaemonPr.exe", "--docker"]