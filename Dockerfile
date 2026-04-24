# Stage 1: build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copia csproj e restaura dependências em camada separada
COPY ["src/OficinaMecanica.API/OficinaMecanica.API.csproj", "src/OficinaMecanica.API/"]
COPY ["src/OficinaMecanica.Infrastructure/OficinaMecanica.Infrastructure.csproj", "src/OficinaMecanica.Infrastructure/"]
COPY ["src/OficinaMecanica.Application/OficinaMecanica.Application.csproj", "src/OficinaMecanica.Application/"]
COPY ["src/OficinaMecanica.Domain/OficinaMecanica.Domain.csproj", "src/OficinaMecanica.Domain/"]

RUN dotnet restore "src/OficinaMecanica.API/OficinaMecanica.API.csproj"

# Copia todo o código e publica
COPY . .
WORKDIR /src/src/OficinaMecanica.API
RUN dotnet publish -c Release -o /app/publish --no-restore

# Stage 2: runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Cria usuário não-root
RUN addgroup --system appgroup && adduser --system appuser --ingroup appgroup || true
USER appuser

COPY --from=build /app/publish ./

# Variáveis recomendadas
ENV ASPNETCORE_URLS=http://+:5000
ENV DOTNET_RUNNING_IN_CONTAINER=true

EXPOSE 5000

ENTRYPOINT ["dotnet", "OficinaMecanica.API.dll"]
