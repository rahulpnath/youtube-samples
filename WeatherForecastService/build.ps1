dotnet lambda package --project-location .\src\WeatherForecast.Api\ `
      --configuration Release `
      --framework net8.0 `
      --output-package artifacts/api.zip

dotnet lambda package --project-location .\src\WeatherForecast.Service\ `
      --configuration Release `
      --framework net8.0 `
      --output-package artifacts/service.zip

# cdklocal deploy -c ASPNETCORE_ENVIRONMENT=LocalStack
