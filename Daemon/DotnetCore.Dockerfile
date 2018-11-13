FROM microsoft/dotnet:2.1-sdk AS build-env
WORKDIR /app

# copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

# copy and build everything else
COPY . ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM microsoft/dotnet:2.1-runtime
WORKDIR /app

EXPOSE 2222

COPY --from=build-env /app/out .

ENTRYPOINT ["dotnet", "DaemonPr.dll"]