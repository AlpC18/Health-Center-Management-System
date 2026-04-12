FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["WellnessAPI.csproj", "./"]
RUN dotnet restore "WellnessAPI.csproj"
COPY . .
RUN dotnet publish "WellnessAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "WellnessAPI.dll"]
