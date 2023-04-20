FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["KafkaConsumer.csproj", "KafkaConsumer/"]
RUN dotnet restore "KafkaConsumer/KafkaConsumer.csproj"
COPY . "KafkaConsumer/"
WORKDIR "/src/KafkaConsumer"
RUN dotnet build "KafkaConsumer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "KafkaConsumer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "KafkaConsumer.dll"]
