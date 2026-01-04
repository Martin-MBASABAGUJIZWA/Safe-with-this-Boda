# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["SafeBoda.Api/SafeBoda.Api.csproj", "SafeBoda.Api/"]
COPY ["SafeBoda.Core/SafeBoda.Core.csproj", "SafeBoda.Core/"]
COPY ["SafeBoda.Application/SafeBoda.Application.csproj", "SafeBoda.Application/"]
COPY ["SafeBoda.Infrastructure/SafeBoda.Infrastructure.csproj", "SafeBoda.Infrastructure/"]

RUN dotnet restore "SafeBoda.Api/SafeBoda.Api.csproj"

# Copy the remaining files and build the project
COPY . .
WORKDIR "/src/SafeBoda.Api"
RUN dotnet publish -c Release -o /app/out

# Stage 2: Run
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "SafeBoda.Api.dll"]
