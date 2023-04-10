# Dockerfile
ARG ProjectDir='src/Http.Client.Options/'
ARG ProjectPath='src/Http.Client.Options/Http.Client.Options.csproj'
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS setup


FROM setup AS source
COPY ["src/Http.Client.Options/Http.Client.Options.csproj", "src/Http.Client.Options/"]
COPY Directory.Build.props .
COPY Directory.Build.targets .
LABEL srclayer=true 

FROM source AS restore
RUN dotnet restore  src/Http.Client.Options/Http.Client.Options.csproj --no-dependencies --verbosity quiet
LABEL restorelayer=true


FROM restore AS build
RUN dotnet build src/Http.Client.Options/Http.Client.Options.csproj --no-restore -c Release 
LABEL buildlayer=true


FROM build AS pack
RUN dotnet pack  src/Http.Client.Options/Http.Client.Options.csproj--no-build -c Release -o /packages
LABEL packlayer=true

FROM pack as local-nuget
RUN dotnet nuget add source /nugets --name local-nuget 
RUN dotnet nuget push -s local-nuget /**/*.nupkg
RUN dotnet nuget push -s local-nuget /**/*.nupkg.symbols.nuspec

VOLUME /nugets 

#ENTRYPOINT ["tail", "-f", "/dev/null"]


# build image
#docker build -t http-options-rc-build .

# build container
#docker run --name http-options-build-container http-options-rc-build

#test net6.0
#docker exec http-options-build-container sh -c 'dotnet test --framework net6.0 --verbosity normal --logger trx --results-directory /output/net6'    

#test net7.0
#docker exec http-options-build-container sh -c 'dotnet test --framework net6.0 --verbosity normal --logger trx --results-directory /output/net6'    
