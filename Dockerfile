# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["LanguageVideoGenerator.Api/LanguageVideoGenerator.Api.csproj", "LanguageVideoGenerator.Api/"]
RUN dotnet restore "LanguageVideoGenerator.Api/LanguageVideoGenerator.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/LanguageVideoGenerator.Api"
RUN dotnet build "LanguageVideoGenerator.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "LanguageVideoGenerator.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LanguageVideoGenerator.Api.dll"]