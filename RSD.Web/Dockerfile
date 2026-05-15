# Stage 1: Tailwind CSS build
FROM node:22-alpine AS tailwind
WORKDIR /src
COPY RSD.Web/package.json ./
RUN npm install
COPY RSD.Web/tailwind.config.js ./
COPY RSD.Web/Styles/ ./Styles/
COPY RSD.Web/Components/ ./Components/
RUN npx tailwindcss -i ./Styles/app.css -o ./wwwroot/css/app.min.css --minify

# Stage 2: .NET restore + publish
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src
COPY RSD.Web/RSD.Web.csproj ./RSD.Web/
RUN dotnet restore RSD.Web/RSD.Web.csproj
COPY RSD.Web/ ./RSD.Web/
COPY --from=tailwind /src/wwwroot/css/app.min.css ./RSD.Web/wwwroot/css/app.min.css
WORKDIR /src/RSD.Web
RUN dotnet publish -c Release -o /app/publish --no-restore

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
ENTRYPOINT ["dotnet", "RSD.Web.dll"]
