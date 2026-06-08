# Stage 1 — BUILD: Full .NET 10 SDK to compile and publish
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project file first — enables Docker layer caching for NuGet restore
COPY ["ST10448420_TechMove_GLMS.csproj", "./"]
RUN dotnet restore "ST10448420_TechMove_GLMS.csproj"

# Copy all source code and publish
COPY . .
RUN dotnet publish "ST10448420_TechMove_GLMS.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

# Stage 2 — FINAL: Lightweight ASP.NET runtime only (no SDK, much smaller image)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "ST10448420_TechMove_GLMS.dll"]