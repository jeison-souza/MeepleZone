# --- Estágio de Build ---
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar os csproj e restaurar dependências
COPY ["HelloWorld/HelloWorld.csproj", "HelloWorld/"]
COPY ["HelloWorld.Client/HelloWorld.Client.csproj", "HelloWorld.Client/"]
RUN dotnet restore "HelloWorld/HelloWorld.csproj"

# Copiar o restante código e compilar
COPY . .
WORKDIR "/src/HelloWorld"
RUN dotnet build "HelloWorld.csproj" -c Release -o /app/build

# --- Estágio de Publish ---
FROM build AS publish
RUN dotnet publish "HelloWorld.csproj" -c Release -o /app/publish /p:UseAppHost=false

# --- Estágio Final / Runtime ---
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HelloWorld.dll"]