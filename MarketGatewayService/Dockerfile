# Use the official .NET 7 SDK image as a build stage
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# Copy csproj files and restore as distinct layers
COPY Common/Common.csproj ./Common/
COPY MarketGatewayService/MarketGatewayService.csproj ./MarketGatewayService/
RUN dotnet restore ./MarketGatewayService/MarketGatewayService.csproj

# Copy everything else and build
COPY Common/. ./Common/
COPY MarketGatewayService/. ./MarketGatewayService/
COPY CommonItems/AvailableTickers.json ./CommonItems/
WORKDIR /src/MarketGatewayService
RUN dotnet publish -c Release -o /app/out

# Use the official .NET runtime image as a runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

# Expose the port the application runs on
EXPOSE 80

# Entry point for the application
ENV ASPNETCORE_ENVIRONMENT Docker
ENV DOTNET_ENVIRONMENT Docker
ENTRYPOINT ["dotnet", "MarketGatewayService.dll"]