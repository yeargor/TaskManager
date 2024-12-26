FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /TaskManager/AuthService
COPY AuthService.csproj ./
RUN dotnet restore "AuthService.csproj"
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
ENV ASPNETCORE_URLS=http://+:5000;http://+:5001
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5000
EXPOSE 5001
ENTRYPOINT ["dotnet", "AuthService.dll"]
