FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /tests
COPY ["tests/Http.Options.UnitTests/Http.Options.UnitTests.csproj", "tests/Http.Options.UnitTests/"]
RUN dotnet restore "tests/Http.Options.UnitTests/Http.Options.UnitTests.csproj"
COPY . .
WORKDIR "/tests/tests/Http.Options.UnitTests"
RUN dotnet build "Http.Options.UnitTests.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Http.Options.UnitTests.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet vstests", "Http.Options.UnitTests.dll", "--logger:trx", "--results-directory:/reports"]
 