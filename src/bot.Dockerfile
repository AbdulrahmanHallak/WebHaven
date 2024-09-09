FROM mcr.microsoft.com/dotnet/sdk:8.0 AS builder

EXPOSE 8080
ARG bot_proj=WebHaven.TelegramBot
ARG schema_proj=WebHaven.DatabaseSchema

WORKDIR /src
COPY ["./${bot_proj}/${bot_proj}.csproj","./${schema_proj}/${schema_proj}.csproj", "./"]
RUN dotnet restore "./${bot_proj}.csproj"

COPY ./${bot_proj} ./${bot_proj}
COPY ./${schema_proj} ./${schema_proj}

RUN mv ${bot_proj}.csproj ./${bot_proj} && \
    mv ${schema_proj}.csproj ${schema_proj} && \
    cd ${bot_proj} && dotnet build ${bot_proj}.csproj -c Release -o /app/build

FROM builder AS publish
RUN cd WebHaven.TelegramBot && dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "WebHaven.TelegramBot.dll"]