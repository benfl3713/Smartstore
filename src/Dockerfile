# Setup Environment
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build-env
WORKDIR /app

COPY ./ ./
WORKDIR /app/Smartstore.Web/
RUN dotnet restore


# Copy everything else and build
RUN dotnet publish -c Release -o /app/Smartstore.Web/deploy

# Generate runtime image
FROM mcr.microsoft.com/dotnet/aspnet:5.0
COPY --from=build-env /app/Smartstore.Web/deploy .
EXPOSE 80
ENTRYPOINT ["dotnet", "Smartstore.Web.dll"]
