# .NET 9 SDK image for build and runtime
FROM mcr.microsoft.com/dotnet/sdk:9.0
WORKDIR /app

# Copy csproj and restore dependencies
COPY ChatTask.ChatService/*.csproj ./ChatTask.ChatService/
COPY ChatTask.Shared/ChatTask.Shared.csproj ./ChatTask.Shared/
RUN dotnet restore ChatTask.ChatService/ChatTask.ChatService.csproj

# Copy everything else and build
COPY . .
RUN dotnet publish ChatTask.ChatService/ChatTask.ChatService.csproj -c Release -o out

# Install EF Core tools
RUN dotnet tool install --global dotnet-ef

# Expose port
EXPOSE 5002

# Start the application
ENTRYPOINT ["dotnet", "out/ChatTask.ChatService.dll"]
