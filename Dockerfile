# ---- Base image (runtime) ----
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# ---- Build image ----
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["InventoryApp/InventoryApp.csproj", "InventoryApp/"]
RUN dotnet restore "InventoryApp/InventoryApp.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/InventoryApp"
RUN dotnet publish "InventoryApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ---- Final image ----
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# Tell ASP.NET Core which port to listen on inside the container
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "InventoryApp.dll"]
