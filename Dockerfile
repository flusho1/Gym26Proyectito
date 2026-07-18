# Usa la imagen oficial de SDK de .NET para compilar
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia el archivo de proyecto y restaura dependencias
COPY ["Gym26.csproj", "./"]
RUN dotnet restore "Gym26.csproj"

# Copia todo el código y compila
COPY . .
RUN dotnet publish "Gym26.csproj" -c Release -o /app/publish

# Usa la imagen de ASP.NET Runtime para ejecutar
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Gym26.dll"]