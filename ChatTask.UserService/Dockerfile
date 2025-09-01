# .NET 9 SDK image for build and runtime
FROM mcr.microsoft.com/dotnet/sdk:9.0
WORKDIR /app

# Copy csproj and restore dependencies
COPY ChatTask.UserService/*.csproj ./ChatTask.UserService/
COPY ChatTask.Shared/ChatTask.Shared.csproj ./ChatTask.Shared/
RUN dotnet restore ChatTask.UserService/ChatTask.UserService.csproj

# Copy everything else and build
COPY . .
RUN dotnet publish ChatTask.UserService/ChatTask.UserService.csproj -c Release -o out

# Install EF Core tools
RUN dotnet tool install --global dotnet-ef

# Expose port
EXPOSE 5001

# Start the application
ENTRYPOINT ["dotnet", "out/ChatTask.UserService.dll"]
