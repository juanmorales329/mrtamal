FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MrTamal.API/MrTamal.API.csproj", "MrTamal.API/"]
COPY ["MrTamal.Shared/MrTamal.Shared.csproj", "MrTamal.Shared/"]
RUN dotnet restore "MrTamal.API/MrTamal.API.csproj"
COPY . .
RUN dotnet publish "MrTamal.API/MrTamal.API.csproj" -c Release -o /app/publish

FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000
ENTRYPOINT ["dotnet", "MrTamal.API.dll"]
