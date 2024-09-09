FROM mcr.microsoft.com/dotnet/sdk:8.0 AS builder

ARG db_migrator_proj=WebHaven.DatabaseMigrator
ARG schema_proj=WebHaven.DatabaseSchema

WORKDIR /src
COPY ["./${db_migrator_proj}/${db_migrator_proj}.csproj","./${schema_proj}/${schema_proj}.csproj", "./"]

RUN dotnet restore "./${db_migrator_proj}.csproj"

COPY ./${db_migrator_proj} ./${db_migrator_proj}
COPY ./${schema_proj} ./${schema_proj}

RUN mv ${db_migrator_proj}.csproj ./${db_migrator_proj} && \
    mv ${schema_proj}.csproj ${schema_proj} && \
    cd ${db_migrator_proj} && dotnet build ${db_migrator_proj}.csproj -c Release -o /app/build

FROM builder AS publish
RUN cd WebHaven.DatabaseMigrator && dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "WebHaven.DatabaseMigrator.dll", "migrateup"]
