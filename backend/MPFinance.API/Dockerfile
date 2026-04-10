# Estágio 1: Build da aplicação
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copia os arquivos de projeto (.csproj) para restaurar as dependências
# O caminho considera que o contexto do comando 'docker build' será a pasta /backend
COPY ["MPFinance.API/MPFinance.API.csproj", "MPFinance.API/"]
COPY ["MPFinance.Application/MPFinance.Application.csproj", "MPFinance.Application/"]
COPY ["MPFinance.Domain/MPFinance.Domain.csproj", "MPFinance.Domain/"]
COPY ["MPFinance.Infrastructure/MPFinance.Infrastructure.csproj", "MPFinance.Infrastructure/"]

# Restaura as dependências (Camada de cache do Docker)
RUN dotnet restore "MPFinance.API/MPFinance.API.csproj"

# Copia todo o código fonte e compila
COPY . .
WORKDIR "/src/MPFinance.API"
RUN dotnet build "MPFinance.API.csproj" -c Release -o /app/build

# Estágio 2: Publicação
FROM build AS publish
RUN dotnet publish "MPFinance.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Estágio 3: Runtime (Imagem final leve)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Define a porta 80 para a API
EXPOSE 80
ENTRYPOINT ["dotnet", "MPFinance.API.dll"]