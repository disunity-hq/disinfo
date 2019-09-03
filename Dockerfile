##
## STAGE: build
##
FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build

# Install Mono
RUN apt update && apt install -y apt-transport-https dirmngr gnupg ca-certificates && \
        apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF && \
        echo "deb https://download.mono-project.com/repo/debian stable-stretch main" | tee /etc/apt/sources.list.d/mono-official-stable.list && \
        apt update && apt install -y mono-devel

WORKDIR /app

# copy csproj and restore as distinct layers
COPY .paket/ ./.paket/
COPY paket.dependencies ./
COPY paket.lock ./
COPY Disunity.Disinfo/Disunity.Disinfo.csproj ./Disunity.Disinfo/
COPY Disunity.Disinfo/paket.references ./Disunity.Disinfo/
RUN mono .paket/paket.exe install

# copy everything else and build app
COPY Disunity.Disinfo ./Disunity.Disinfo/
WORKDIR /app/Disunity.Disinfo
RUN dotnet publish -p:SolutionDir=$(pwd) -c Release -o out Disunity.Disinfo.csproj


##
## STAGE: runtime
##
FROM mcr.microsoft.com/dotnet/core/runtime:2.2 as runtime
WORKDIR /app
COPY --from=build /app/Disunity.Disinfo/out ./
RUN mkdir /db
ENTRYPOINT ["dotnet", "Disunity.Disinfo.dll"]
