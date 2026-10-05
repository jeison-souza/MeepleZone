# --- Étape de build ---
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copier les fichiers csproj et restaurer les dépendances
COPY ["MeepleZone/MeepleZone/MeepleZone.csproj", "MeepleZone/MeepleZone/"]
COPY ["MeepleZone/MeepleZone.Client/MeepleZone.Client.csproj", "MeepleZone/MeepleZone.Client/"]
RUN dotnet restore "MeepleZone/MeepleZone/MeepleZone.csproj"

# Copier le reste du code
COPY . .
WORKDIR "/src/MeepleZone/MeepleZone"

# --- Étape de publication (compile et publie en une seule passe) ---
FROM build AS publish
RUN dotnet publish "MeepleZone.csproj" -c Release -o /app/publish --no-restore /p:UseAppHost=false

# --- Étape finale / exécution ---
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080

COPY --from=publish /app/publish .

# Exécuter sans les droits root (utilisateur "app" fourni par l'image)
USER $APP_UID
ENTRYPOINT ["dotnet", "MeepleZone.dll"]
