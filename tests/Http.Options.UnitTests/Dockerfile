FROM mcr.microsoft.com/dotnet/sdk:7.0 AS base
RUN dotnet tool install dotnet-reportgenerator-globaltool --tool-path /dotnetglobaltools

FROM base AS restore
COPY ["/nugets", "/nugets"]

RUN dotnet nuget add source /nugets --name local-nuget 

COPY Directory.Build.props .
COPY Directory.Build.targets .
COPY ["tests/Http.Options.UnitTests/Http.Options.UnitTests.csproj", "tests/Http.Options.UnitTests/"]
RUN dotnet restore --no-dependencies "tests/Http.Options.UnitTests/Http.Options.UnitTests.csproj" --verbosity quiet

FROM restore AS build
WORKDIR .
RUN dotnet build --no-restore --no-dependencies "tests/Http.Options.UnitTests/Http.Options.UnitTests.csproj" -c Release --verbosity quiet

FROM build AS net6
RUN dotnet publish --no-build --no-restore "tests/Http.Options.UnitTests/Http.Options.UnitTests.csproj" -c Release -o /out/net6.0  --framework net6.0 --self-contained  

FROM build AS net7
RUN dotnet publish  --no-build --no-restore "tests/Http.Options.UnitTests/Http.Options.UnitTests.csproj" -c Release -o /out/net7.0  --framework net7.0 --self-contained  

RUN dotnet vstest --Parallel --Platform:x64 --logger:trx --results-directory:/reports (Get-ChildItem -Recurse *\**\net6.0\**\*Tests.dll)
RUN dotnet vstest --Parallel --Platform:x64 --logger:trx --results-directory:/reports (Get-ChildItem -Recurse *\**\net7.0\**\*Tests.dll)
 #create a sh entrypoint that will run the tests for the requested framework found in the /out directory
# e.g. docker run --rm -v $(pwd)/reports:/reports tests:latest net6.0
# e.g. docker run --rm -v $(pwd)/reports:/reports tests:latest net7.0

#ENTRYPOINT ["/root/.dotnet/tools/dotnet-vstest", "/out/$1/Http.Options.UnitTests.dll", "--logger:trx", "--results-directory:/reports"]
#ENTRYPOINT [
#"dotnet vstest --Parallel --Platform:x64 --logger:trx --results-directory:/reports (Get-ChildItem -Recurse src\**\bin\**\net6.0\**\*Tests.dll)",
#"&& dotnet vstest --Parallel --Platform:x64 --logger:trx --results-directory:/reports (Get-ChildItem -Recurse src\**\bin\**\net7.0\**\*Tests.dll)"]

VOLUME /reports 

