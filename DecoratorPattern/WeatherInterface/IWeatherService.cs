namespace DecoratorPattern.WeatherInterface
{
    public interface IWeatherService
    {

        CurrentWeather GetCurrentWeather(String location);


        LocationForecast GetForecast(String location);


    }
}
