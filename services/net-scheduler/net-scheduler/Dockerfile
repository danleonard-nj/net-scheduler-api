#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["net-scheduler.csproj", "."]
RUN dotnet restore "net-scheduler.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "net-scheduler.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "net-scheduler.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN apt-get update -
RUN apt-get update && apt-get dist-upgrade -y
ENTRYPOINT ["dotnet", "net-scheduler.dll"]