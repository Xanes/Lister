# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 443

# Install OpenSSL to generate self-signed cert
RUN apt-get update && apt-get install -y openssl

# Generate a self-signed certificate
RUN mkdir -p /https && \
    openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
    -keyout /https/aspnetapp.key -out /https/aspnetapp.crt \
    -subj "/CN=localhost"

# Create PFX file from cert and key
RUN openssl pkcs12 -export -out /https/aspnetapp.pfx \
    -inkey /https/aspnetapp.key -in /https/aspnetapp.crt \
    -passout pass:CertPassword123


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
COPY ["Dmain/Domain.csproj", "Dmain/"]
COPY ["PDFReader/PDFReader.csproj", "PDFReader/"]
RUN dotnet restore "./PDFReader/PDFReader.csproj"
COPY . .
WORKDIR "/src/PDFReader"
RUN dotnet build "./PDFReader.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./PDFReader.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PDFReader.dll"]