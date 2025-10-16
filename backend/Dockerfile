FROM mcr.microsoft.com/dotnet/sdk:8.0 AS base
WORKDIR /src

COPY ["src/Fundo.Applications.WebApi/Fundo.Applications.WebApi.csproj","Fundo.Applications.WebApi/"]
COPY ["src/Fundo.Applications.Domain/Fundo.Applications.Domain.csproj", "Fundo.Applications.Domain/"]
COPY ["src/Fundo.Applications.Infrastructure/Fundo.Applications.Infrastructure.csproj", "Fundo.Applications.Infrastructure/"]
COPY ["src/Fundo.Applications.Application/Fundo.Applications.Application.csproj", "Fundo.Applications.Application/"]


RUN dotnet restore "Fundo.Applications.WebApi/Fundo.Applications.WebApi.csproj"

COPY src/ .

RUN dotnet publish "Fundo.Applications.WebApi/Fundo.Applications.WebApi.csproj" -c Release -o /app/publish


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
COPY --from=base /app/publish .
ENV ASPNETCORE_URLS=http://+:80
ENTRYPOINT ["dotnet","Fundo.Applications.WebApi.dll"]