
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src


COPY ["ProductManagement.API/ProductManagement.API.csproj", "ProductManagement.API/"]
COPY ["ProductManagement.Application/ProductManagement.Application.csproj", "ProductManagement.Application/"]
COPY ["ProductManagement.Domain/ProductManagement.Domain.csproj", "ProductManagement.Domain/"]
COPY ["ProductManagement.Infrastructure/ProductManagement.Infrastructure.csproj", "ProductManagement.Infrastructure/"]


RUN dotnet restore "ProductManagement.API/ProductManagement.API.csproj"


COPY . .
WORKDIR "/src/ProductManagement.API"


RUN dotnet publish "ProductManagement.API.csproj" -c Release -o /app/publish


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app


COPY --from=build /app/publish .


EXPOSE 8080

ENTRYPOINT ["dotnet", "ProductManagement.API.dll"]