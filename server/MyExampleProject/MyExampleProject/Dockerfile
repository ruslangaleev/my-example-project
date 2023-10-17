#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["MyExampleProject/MyExampleProject.csproj", "MyExampleProject/"]
RUN dotnet restore "MyExampleProject/MyExampleProject.csproj"
COPY . .
WORKDIR "/src/MyExampleProject"
RUN dotnet build "MyExampleProject.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MyExampleProject.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyExampleProject.dll"]