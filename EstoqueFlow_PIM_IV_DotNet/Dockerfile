FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/EstoqueFlow.Web/EstoqueFlow.Web.csproj ./EstoqueFlow.Web/
RUN dotnet restore ./EstoqueFlow.Web/EstoqueFlow.Web.csproj

COPY src/EstoqueFlow.Web/ ./EstoqueFlow.Web/
WORKDIR /src/EstoqueFlow.Web
RUN dotnet publish EstoqueFlow.Web.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "EstoqueFlow.Web.dll"]
