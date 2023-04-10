FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["tests/TestRunner/TestRunner.csproj", "tests/TestRunner/"]
RUN dotnet restore "tests/TestRunner/TestRunner.csproj"
COPY . .
WORKDIR "/src/tests/TestRunner"
RUN dotnet build "TestRunner.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TestRunner.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TestRunner.dll"]
