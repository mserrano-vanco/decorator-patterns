using System.Diagnostics;

namespace DecoratorPattern.WeatherInterface
{
    public class WeatherServiceLoggingDecorator : IWeatherService
    {
        private readonly IWeatherService innerService;
        private readonly ILogger<WeatherServiceLoggingDecorator> logger;

        public WeatherServiceLoggingDecorator(IWeatherService service, ILogger<WeatherServiceLoggingDecorator> logger)
        {
            innerService = service;
            this.logger = logger;
        }

        public CurrentWeather GetCurrentWeather(string location)
        {
            Stopwatch sw = Stopwatch.StartNew();
            CurrentWeather cw = innerService.GetCurrentWeather(location);
            sw.Stop();
            long elapsedMillis = sw.ElapsedMilliseconds;
            logger.LogWarning("Retrieved weather data for {location} - Elapsed ms: {} {@currentWeather}", 
                location, elapsedMillis, cw);

            return cw;
        }

        public LocationForecast GetForecast(string location)
        {
            Stopwatch sw = Stopwatch.StartNew();
            LocationForecast forecast = innerService.GetForecast(location);
            sw.Stop();
            long elapsedMillis = sw.ElapsedMilliseconds;
            logger.LogWarning("Retrieved forecast data for {location} - Elapsed ms: {} {@currentWeather}",
                location, elapsedMillis, forecast);

            return forecast;
        }
    }
}
