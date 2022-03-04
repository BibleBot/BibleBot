ARG OS=alpine
FROM mcr.microsoft.com/dotnet/sdk:5.0-${OS} AS build-env
WORKDIR /source

COPY *.csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:5.0-${OS}
WORKDIR /app
COPY --from=build-env /app .
ENTRYPOINT [ "dotnet", "BibleBot.AutomaticServices.dll" ]