FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/PaymentGateway.Api/PaymentGateway.Api.csproj src/PaymentGateway.Api/
RUN dotnet restore src/PaymentGateway.Api/PaymentGateway.Api.csproj

COPY src/PaymentGateway.Api/ src/PaymentGateway.Api/
RUN dotnet publish src/PaymentGateway.Api/PaymentGateway.Api.csproj \
    -c Release --no-restore -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "PaymentGateway.Api.dll"]
