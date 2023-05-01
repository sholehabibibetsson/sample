
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

COPY Technical_assignment Technical_assignment

WORKDIR /Technical_assignment
RUN dotnet restore

RUN dotnet build "Technical_assignment.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Technical_assignment.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Technical_assignment.dll"]