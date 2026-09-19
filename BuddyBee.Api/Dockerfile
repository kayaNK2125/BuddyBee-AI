FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["BuddyBee.Api/BuddyBee.Api.csproj", "BuddyBee.Api/"]
RUN dotnet restore "BuddyBee.Api/BuddyBee.Api.csproj"
COPY . .
WORKDIR "/src/BuddyBee.Api"
RUN dotnet publish "BuddyBee.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Railway provides PORT dynamically at runtime (e.g. PORT=8080 or random port).
# In modern ASP.NET Core (.NET 8+), ASPNETCORE_HTTP_PORTS binds to ports.
# We set the entrypoint via shell form to evaluate ${PORT:-8080} dynamically.
ENV PORT=8080
EXPOSE 8080

ENTRYPOINT ["sh", "-c", "ASPNETCORE_HTTP_PORTS=${PORT:-8080} dotnet BuddyBee.Api.dll"]
