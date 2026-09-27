FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/DotNetVisualLab.Web/DotNetVisualLab.Web.csproj", "src/DotNetVisualLab.Web/"]
RUN dotnet restore "src/DotNetVisualLab.Web/DotNetVisualLab.Web.csproj"

COPY . .
WORKDIR "/src/src/DotNetVisualLab.Web"
RUN dotnet publish "DotNetVisualLab.Web.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "DotNetVisualLab.Web.dll"]
