# 1) Build your Function
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["AuthenticationApp.AzureFunc.EmailLogin.csproj", "./"]
RUN dotnet restore "./AuthenticationApp.AzureFunc.EmailLogin.csproj"

COPY . .
RUN dotnet publish "./AuthenticationApp.AzureFunc.EmailLogin.csproj" \
    -c Release -o /app/publish

# 2) Runtime image
FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated8.0 AS final
WORKDIR /home/site/wwwroot

# copy your published function
COPY --from=build /app/publish .

# enable console logging (optional)
ENV AzureFunctionsJobHost__Logging__Console__IsEnabled=true
# ensure isolated worker is selected
ENV FUNCTIONS_WORKER_RUNTIME=dotnet-isolated

