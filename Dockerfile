FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY LMStore.slnx .
COPY src/LMStore.Domain/LMStore.Domain.csproj src/LMStore.Domain/
COPY src/LMStore.Application/LMStore.Application.csproj src/LMStore.Application/
COPY src/LMStore.Infrastructure/LMStore.Infrastructure.csproj src/LMStore.Infrastructure/
COPY src/LMStore.Api/LMStore.Api.csproj src/LMStore.Api/
COPY tests/LMStore.Tests/LMStore.Tests.csproj tests/LMStore.Tests/
RUN dotnet restore src/LMStore.Api/LMStore.Api.csproj

COPY . .
RUN dotnet publish src/LMStore.Api/LMStore.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

EXPOSE 8080
ENTRYPOINT ["dotnet", "LMStore.Api.dll"]
