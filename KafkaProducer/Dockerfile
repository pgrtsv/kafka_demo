FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["KafkaProducer.csproj", "KafkaProducer/"]
RUN dotnet restore "KafkaProducer/KafkaProducer.csproj"
COPY . "KafkaProducer/"
WORKDIR "/src/KafkaProducer"
RUN dotnet build "KafkaProducer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "KafkaProducer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "KafkaProducer.dll"]
