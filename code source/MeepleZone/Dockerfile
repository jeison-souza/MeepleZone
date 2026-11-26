# --- Étape de build ---
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copier le code et restaurer les dépendances
COPY . .
RUN dotnet restore "Site/Site/MeepleZone.Site.csproj"
WORKDIR "/src/Site/Site"

# --- Étape de publication (compile et publie en une seule passe) ---
FROM build AS publish
RUN dotnet publish "MeepleZone.Site.csproj" -c Release -o /app/publish --no-restore /p:UseAppHost=false

# --- Étape finale / exécution ---
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080

COPY --from=publish /app/publish .

# Numéro de version et commit, fournis par le pipeline et lus par l'application (« dev » hors pipeline)
ARG VERSION_NUMERO=dev
ARG VERSION_COMMIT=
ENV Version__Numero=$VERSION_NUMERO \
    Version__Commit=$VERSION_COMMIT

# Exécuter sans les droits root (utilisateur "app" fourni par l'image)
USER $APP_UID
ENTRYPOINT ["dotnet", "MeepleZone.Site.dll"]
