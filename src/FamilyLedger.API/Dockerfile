FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/FamilyLedger.API/FamilyLedger.API.csproj", "src/FamilyLedger.API/"]
COPY ["src/FamilyLedger.Application/FamilyLedger.Application.csproj", "src/FamilyLedger.Application/"]
COPY ["src/FamilyLedger.Domain/FamilyLedger.Domain.csproj", "src/FamilyLedger.Domain/"]
COPY ["src/FamilyLedger.Infrastructure/FamilyLedger.Infrastructure.csproj", "src/FamilyLedger.Infrastructure/"]
RUN dotnet restore "src/FamilyLedger.API/FamilyLedger.API.csproj"
COPY . .
RUN dotnet publish "src/FamilyLedger.API/FamilyLedger.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FamilyLedger.API.dll"]
