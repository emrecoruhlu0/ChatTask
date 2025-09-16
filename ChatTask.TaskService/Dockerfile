FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5004

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["ChatTask.TaskService/ChatTask.TaskService.csproj", "ChatTask.TaskService/"]
COPY ["ChatTask.Shared/ChatTask.Shared.csproj", "ChatTask.Shared/"]
RUN dotnet restore "ChatTask.TaskService/ChatTask.TaskService.csproj"
COPY . .
WORKDIR "/src/ChatTask.TaskService"
RUN dotnet build "ChatTask.TaskService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ChatTask.TaskService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ChatTask.TaskService.dll"]
