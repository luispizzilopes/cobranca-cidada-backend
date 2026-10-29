FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Cobranca.Cidada.Presentation/Cobranca.Cidada.Presentation.csproj", "Cobranca.Cidada.Presentation/"]
COPY ["Cobranca.Cidada.CrossCutting/Cobranca.Cidada.CrossCutting.csproj", "Cobranca.Cidada.CrossCutting/"]
COPY ["Cobranca.Cidada.Application/Cobranca.Cidada.Application.csproj", "Cobranca.Cidada.Application/"]
COPY ["Cobranca.Cidada.Domain/Cobranca.Cidada.Domain.csproj", "Cobranca.Cidada.Domain/"]
COPY ["Cobranca.Cidada.BuildingBlocks/Cobranca.Cidada.BuildingBlocks.csproj", "Cobranca.Cidada.BuildingBlocks/"]
COPY ["Cobranca.Cidada.Infrastructure/Cobranca.Cidada.Infrastructure.csproj", "Cobranca.Cidada.Infrastructure/"]
COPY ["Cobranca.Cidada.BackgroundJobs/Cobranca.Cidada.BackgroundJobs.csproj", "Cobranca.Cidada.BackgroundJobs/"]
RUN dotnet restore "Cobranca.Cidada.Presentation/Cobranca.Cidada.Presentation.csproj"
COPY . .
WORKDIR "/src/Cobranca.Cidada.Presentation"
RUN dotnet build "./Cobranca.Cidada.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Cobranca.Cidada.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Cobranca.Cidada.Presentation.dll"]
