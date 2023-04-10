FROM ghcr.io/catthehacker/ubuntu:act-latest
RUN node -v
RUN npm -v
RUN npm install -g npm@latest
RUN npm cache clean -f    
RUN npm install -g n    
RUN n stable

RUN export DEBIAN_FRONTEND=noninteractive \
    apt-get update \
    # Install prerequisites
    && apt-get install -y --no-install-recommends \
       wget \
       ca-certificates \
    \
    # Install Microsoft package feed
    && wget -q https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb \
    && dpkg -i packages-microsoft-prod.deb \
    && rm packages-microsoft-prod.deb \
    \
    # Install .NET
    && apt-get update \
    && apt-get install -y --no-install-recommends \
        dotnet-sdk-6.0 \
    && apt-get install -y --no-install-recommends \
            dotnet-sdk-7.0 \
    \
    # Cleanup
    && rm -rf /var/lib/apt/lists/*
#
## install dotnet sdk 7.0 and 6.0
#
#RUN apt-get update \
#    && DEBIAN_FRONTEND=noninteractive apt-get install -y --no-install-recommends \
#        ca-certificates \
#        \
#        # .NET dependencies
#        libc6 \
#        libgcc1 \
#        libgssapi-krb5-2 \
#        libicu66 \
#        libssl1.1 \
#        libstdc++6 \
#        zlib1g \
#    && rm -rf /var/lib/apt/lists/*
#
## Install .NET
#ENV DOTNET_VERSION=6.0.0
#
#RUN curl -fSL --output dotnet.tar.gz https://dotnetcli.azureedge.net/dotnet/Runtime/$DOTNET_VERSION/dotnet-runtime-$DOTNET_VERSION-linux-x64.tar.gz \
#    && dotnet_sha512='6a1ae878efdc9f654e1914b0753b710c3780b646ac160fb5a68850b2fd1101675dc71e015dbbea6b4fcf1edac0822d3f7d470e9ed533dd81d0cfbcbbb1745c6c' \
#    && echo "$dotnet_sha512 dotnet.tar.gz" | sha512sum -c - \
#    && mkdir -p /usr/share/dotnet \
#    && tar -zxf dotnet.tar.gz -C /usr/share/dotnet \
#    && rm dotnet.tar.gz \
#    && ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet
#
## install also dotnet 7.0.0
#ENV DOTNET_VERSION=7.0.0
#RUN curl -fSL --output dotnet.tar.gz https://dotnetcli.azureedge.net/dotnet/Runtime/$DOTNET_VERSION/dotnet-runtime-$DOTNET_VERSION-linux-x64.tar.gz \
#    && dotnet_sha512='6a1ae878efdc9f654e1914b0753b710c3780b646ac160fb5a68850b2fd1101675dc71e015dbbea6b4fcf1edac0822d3f7d470e9ed533dd81d0cfbcbbb1745c6c' \
#    && echo "$dotnet_sha512 dotnet.tar.gz" | sha512sum -c - \
#    && mkdir -p /usr/share/dotnet \
#    && tar -zxf dotnet.tar.gz -C /usr/share/dotnet \
#    && rm dotnet.tar.gz \
#    && ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet

RUN dotnet --list-sdks
RUN dotnet --list-runtimes
RUN dotnet --version
RUN dotnet tool install --global dotnet-sonarscanner
RUN dotnet tool install --global dotnet-reportgenerator-globaltool

#FROM mcr.microsoft.com/dotnet/sdk:7.0 
#install node 18 and npm ensure fs/promises is available globally
# install node version 18 
#RUN curl -fsSL https://deb.nodesource.com/setup_18.x | sudo -E bash -
#RUN apt-get update && apt-get install -y nodejs npm
#RUN node -v
#RUN defaultVersion=18 &&\
#    curl -L https://raw.githubusercontent.com/tj/n/master/bin/n -o ~/n &&\
#    bash ~/n ${defaultVersion}

#install actions
#RUN npm install --save-dev @actions/core
#RUN npm install --save-dev @actions/github
#RUN npm install --save-dev @actions/io
#RUN npm install --save-dev @actions/exec
#RUN npm install --save-dev @actions/cache
#RUN npm install --save-dev @actions/artifact
#RUN npm install --save-dev @actions/tool-cache
#RUN npm install --save-dev @actions/http-client
#install dotnet

#FROM mcr.microsoft.com/dotnet/sdk:6.0
##CMD echo "hello ${GITHUB_WORKSPACE}" 
##CMD echo ${GITHUB_WORKSPACE}
##WORKDIR ${GITHUB_WORKSPACE:-./}
#COPY ../**/*.csproj ./
##CMD mkdir -p src
##COPY $GITHUB_WORKSPACE/*.csproj $GITHUB_WORKSPACE/src/
##WORKDIR $GITHUB_WORKSPACE/src
#RUN dotnet restore
#COPY ../**/* ./
##COPY $GITHUB_WORKSPACE $GITHUB_WORKSPACE/src
#RUN dotnet build --no-restore -c Release
#RUN dotnet test --no-build -c Release --verbosity normal --logger "GitHubActions;report-warnings=false" --results-directory /github/workspace/test-results

