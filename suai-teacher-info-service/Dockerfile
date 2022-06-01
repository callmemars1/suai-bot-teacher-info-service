FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /teacher-info-service
COPY . ./
RUN dotnet restore
RUN dotnet publish -c release -o /teacher-info-service --no-restore

FROM mcr.microsoft.com/dotnet/runtime:6.0
WORKDIR /teacher-info-service
COPY --from=build-env /teacher-info-service/bin/Release/net6.0/ .
ENTRYPOINT ["dotnet", "suai-teacher-info-service.dll"]