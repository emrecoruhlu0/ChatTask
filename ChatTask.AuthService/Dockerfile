# .NET 9 SDK image for build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy csproj and restore dependencies
COPY ChatTask.AuthService/*.csproj ./ChatTask.AuthService/
COPY ChatTask.Shared/ChatTask.Shared.csproj ./ChatTask.Shared/
RUN dotnet restore ChatTask.AuthService/ChatTask.AuthService.csproj

# Copy everything else and build
COPY . .
RUN dotnet publish ChatTask.AuthService/ChatTask.AuthService.csproj -c Release -o out

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/out .

# Expose port
EXPOSE 5003

ENTRYPOINT ["dotnet", "ChatTask.AuthService.dll"]
