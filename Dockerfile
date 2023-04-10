# Dockerfile.buildpack
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS setup

FROM setup AS source
RUN mkdir -p /lib
COPY . /lib

FROM source AS restore
WORKDIR /lib
RUN dotnet restore 

FROM restore AS build
WORKDIR /lib
RUN dotnet build --no-restore -c Release

FROM build AS pack
RUN dotnet pack --no-build -c Release -o /packages

FROM pack as local-nuget
RUN dotnet nuget add source /nugets --name local-nuget 
RUN dotnet nuget push -s local-nuget /packages/*.nupkg

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
