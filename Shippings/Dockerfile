FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["Shippings/Shippings.csproj", "Shippings/"]
RUN dotnet restore "Shippings/Shippings.csproj"
COPY . .
WORKDIR "/src/Shippings"
RUN dotnet build "Shippings.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Shippings.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Shippings.dll"]