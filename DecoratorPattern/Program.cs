using Serilog.Formatting.Compact;
using Serilog;
using DecoratorPattern.WeatherInterface;
using DecoratorPattern.OpenWeatherMap;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.DataProtection.KeyManagement;

#pragma warning disable CS8604 // Possible null reference argument.

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddMemoryCache();


string apiKey = builder.Configuration["OpenWeatherMapApiKey"];
builder.Services.AddScoped<IWeatherService>(serviceProvider => new WeatherService(apiKey));
builder.Services.Decorate<IWeatherService>(
    (inner, provider) => 
    new WeatherServiceLoggingDecorator(inner, provider.GetService<ILogger<WeatherServiceLoggingDecorator>>()));
builder.Services.Decorate<IWeatherService>(
    (inner, provider) => 
    new WeatherServiceCachingDecorator(inner, provider.GetService<IMemoryCache>()));

/*
 * The code above does the same as this one belows
builder.Services.AddScoped(serviceProvider =>
{
    String apiKey = builder.Configuration["OpenWeatherMapApiKey"];
    var logger = serviceProvider.GetService<ILogger<WeatherServiceLoggingDecorator>>();
    var memoryCache = serviceProvider.GetService<IMemoryCache>();

    IWeatherService concreteService = new WeatherService(apiKey);
    IWeatherService withLoggingDecorator = new WeatherServiceLoggingDecorator(concreteService, logger);
    IWeatherService withCachingDecorator = new WeatherServiceCachingDecorator(withLoggingDecorator, memoryCache);
    return withCachingDecorator;
});*/

builder.Host.UseSerilog((ctx, lc) => lc
    .WriteTo.Console()
    .WriteTo.Seq("http://localhost:7007"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

var logfile = Path.Combine(Path.GetTempPath(), "DecoratorDesignPatternLog.log");

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.File(new RenderedCompactJsonFormatter(), logfile)
    .WriteTo.Console()
    .CreateLogger();
try
{
    Log.Information("Starting up");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    Log.CloseAndFlush();
}


