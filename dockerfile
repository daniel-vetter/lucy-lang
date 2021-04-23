FROM mcr.microsoft.com/dotnet/sdk:5.0 as build
COPY /src /src
WORKDIR /src/compiler
RUN dotnet build