# Dockerfile.test
ARG dotnet_version=7.0
ARG logger='trx;LogFileName=TestResults.trx'
ARG coverage_dir=/out/coverage/
ARG results_dir=/out/testresults/

FROM httpoptions-build:latest AS nugets
FROM mcr.microsoft.com/dotnet/sdk:${dotnet_version} AS build
COPY --from=nugets /nugets /nugets
RUN dotnet nuget add source /nugets --name local-nuget 
COPY /app /app
WORKDIR /app 


FROM build as testrunner
COPY --from=build /test /test

WORKDIR /test/
RUN dotnet tool install dotnet-reportgenerator-globaltool --tool-path /dotnetglobaltools
LABEL unittestlayer=true
RUN "dotnet test" \
    "-c Release"  \
    "--framework net${dotnet_version}" \
    "--logger ${logger}" \
    "--results-directory ${results_dir}" \
    "--verbosity" "normal"\
    "/p:CollectCoverage=true" \
    "/p:CoverletOutputFormat=cobertura" \
    "/p:CoverletOutput=${coverage_dir}"  \
    "/p:Exclude=[Test.*]*"  \
    
    
RUN /dotnetglobaltools/reportgenerator  \
    "-reports:/${coverage_dir}/coverage.cobertura.xml" \
    "-targetdir:/${coverage_dir}"  \
    "-reporttypes:HTMLInline;HTMLChart"

#RUN dotnet test -c Release --framework net${dotnet_version} --verbosity normal --logger ${logger} --results-directory ${results_dir}

#RUN echo '#!/bin/sh\n\
#for file in $(find . -name "*.csproj" -type f); do\n\
#    echo "Listing packages for $file"\n\
#    dotnet list $file package\n\
#done' > /app/list-packages.sh && chmod +x /app/list-packages.sh

    
