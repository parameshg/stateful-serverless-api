FROM public.ecr.aws/lambda/dotnet:6 AS base

FROM mcr.microsoft.com/dotnet/sdk:6.0-bullseye-slim as build
WORKDIR "/src"
COPY ["source/Api.csproj", "."]
RUN dotnet restore "./Api.csproj"

WORKDIR "/src"
COPY ./source .
RUN dotnet build "Api.csproj" --configuration Release --output /app/build

FROM build AS publish
RUN dotnet publish "Api.csproj" --configuration Release --runtime linux-x64 --self-contained false --output /app/publish -p:PublishReadyToRun=true

FROM base AS final
WORKDIR /var/task
COPY --from=publish /app/publish .