# Use the official .NET 7 SDK image as a build stage
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# Copy csproj files and restore as distinct layers
COPY Common/Common.csproj ./Common/
COPY PriceUpdateHandlerService/PriceUpdateHandlerService.csproj ./PriceUpdateHandlerService/
RUN dotnet restore ./PriceUpdateHandlerService/PriceUpdateHandlerService.csproj

# Copy everything else and build
COPY Common/. ./Common/
COPY PriceUpdateHandlerService/. ./PriceUpdateHandlerService/
COPY CommonItems/AvailableTickers.json ./CommonItems/
WORKDIR /src/PriceUpdateHandlerService
RUN dotnet publish -c Release -o /app/out

# Use the official .NET runtime image as a runtime stage
FROM mcr.microsoft.com/dotnet/runtime:7.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

# Entry point for the application
ENV ASPNETCORE_ENVIRONMENT Docker
ENV DOTNET_ENVIRONMENT Docker
ENTRYPOINT ["dotnet", "PriceUpdateHandlerService.dll"]