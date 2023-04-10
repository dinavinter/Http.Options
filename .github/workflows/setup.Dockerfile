## use act to pull https://github.com/actions/setup-dotnet from gitub and run it in the docker , this sholud be a base image for other workfolows
FROM ghcr.io/catthehacker/ubuntu:act-latest
RUN curl -s https://raw.githubusercontent.com/nektos/act/master/install.sh | sudo bash
ARG DOTNET_VERSION=7.0.0
RUN git pull https://github.com/actions/setup-dotnet.git 
RUN act 

COPY .actrc /
RUN mv /.actrc ~/.actrc

COPY .act/. .act/.
