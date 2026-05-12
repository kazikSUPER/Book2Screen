FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS base
RUN apk add --no-cache krb5-libs curl
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
RUN apk add --no-cache krb5-libs
WORKDIR /src
COPY ["Book2Screen.csproj", "./"]
RUN dotnet restore "Book2Screen.csproj"
COPY . .
# Force Release configuration here
RUN dotnet build "Book2Screen.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Book2Screen.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
# This ensures only the published files are in the final image
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Book2Screen.dll"]
