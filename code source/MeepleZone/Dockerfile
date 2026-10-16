# --- Étape de build ---
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copier les fichiers csproj et restaurer les dépendances
COPY ["Site/Site/MeepleZone.Site.csproj", "Site/Site/"]
COPY ["Site/Site.Client/MeepleZone.Site.Client.csproj", "Site/Site.Client/"]
RUN dotnet restore "Site/Site/MeepleZone.Site.csproj"

# Copier le reste du code
COPY . .
WORKDIR "/src/Site/Site"

# --- Étape de publication (compile et publie en une seule passe) ---
FROM build AS publish
RUN dotnet publish "MeepleZone.Site.csproj" -c Release -o /app/publish --no-restore /p:UseAppHost=false

# --- Étape finale / exécution ---
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080

COPY --from=publish /app/publish .

# Exécuter sans les droits root (utilisateur "app" fourni par l'image)
USER $APP_UID
ENTRYPOINT ["dotnet", "MeepleZone.Site.dll"]
