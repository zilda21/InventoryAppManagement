# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy everything in the repo into the container
COPY . .

# Restore and publish the web app
RUN dotnet restore "InventoryApp/InventoryApp.csproj"
RUN dotnet publish "InventoryApp/InventoryApp.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Copy the published output from the build stage
COPY --from=build /app/publish .

# Render expects the app to listen on 0.0.0.0:8080
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "InventoryApp.dll"]
