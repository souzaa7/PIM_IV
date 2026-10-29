# ---------- Etapa 1: compila a API (imagem com o SDK do .NET)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY AprenderMais.Dominio/ AprenderMais.Dominio/
COPY AprenderMais.Aplicacao/ AprenderMais.Aplicacao/
COPY AprenderMais.Infraestrutura/ AprenderMais.Infraestrutura/
COPY AprenderMais.Api/ AprenderMais.Api/
RUN dotnet publish AprenderMais.Api/AprenderMais.Api.csproj -c Release -o /app

# ---------- Etapa 2: imagem final, só com o runtime (bem menor)
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
HEALTHCHECK --interval=30s --timeout=5s CMD wget -qO- http://localhost:8080/saude || exit 1
ENTRYPOINT ["dotnet", "AprenderMais.Api.dll"]
