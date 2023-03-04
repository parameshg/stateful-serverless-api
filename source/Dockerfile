FROM public.ecr.aws/lambda/dotnet:6 AS base

FROM mcr.microsoft.com/dotnet/sdk:6.0-bullseye-slim as build
WORKDIR "/src"
COPY ["Counter.csproj", "."]
RUN dotnet restore "./Counter.csproj"

WORKDIR "/src"
COPY . .
RUN dotnet build "Counter.csproj" --configuration Release --output /app/build

FROM build AS publish
RUN dotnet publish "Counter.csproj" --configuration Release --runtime linux-x64 --self-contained false --output /app/publish -p:PublishReadyToRun=true

FROM base AS final
WORKDIR /var/task
COPY --from=publish /app/publish .