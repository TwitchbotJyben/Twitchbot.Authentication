FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
ARG NUGET_AUTH_TOKEN

LABEL "org.opencontainers.image.source"="https://github.com/TwitchbotJyben/Twitchbot.Services.Authentication"

WORKDIR /src
COPY Twitchbot.Services.Authentication.csproj .

COPY ["nuget.config", ""]
RUN sed -i -e "s/PW/$NUGET_AUTH_TOKEN/g" nuget.config
RUN dotnet restore
COPY . .
RUN dotnet publish -c release -o /app


FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "Twitchbot.Services.Authentication.dll"]
