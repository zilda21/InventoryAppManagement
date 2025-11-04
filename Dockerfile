# =========================
# 1) Build stage
# =========================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Work in /src inside the container
WORKDIR /src

# Copy everything from the repo into /src
COPY . .

# Restore and publish the app from the csproj in the repo root
RUN dotnet restore "InventoryApp.csproj"
RUN dotnet publish "InventoryApp.csproj" -c Release -o /app/publish

# =========================
# 2) Runtime stage
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

WORKDIR /app

# Copy published output from build stage
COPY --from=build /app/publish .

# Render listens on port 8080 by default
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Run the app
ENTRYPOINT ["dotnet", "InventoryApp.dll"]
