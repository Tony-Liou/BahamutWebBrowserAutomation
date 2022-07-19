using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Polly;
using Serilog;
using WebBrowserAutomation;
using WebBrowserAutomation.Pages;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

using IHost host = Host.CreateDefaultBuilder(args).Build();
IConfiguration config = host.Services.GetRequiredService<IConfiguration>();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(config)
    .CreateLogger();

Log.Verbose("正在測試巴哈是否正常");
var operatingTask = Policy
    .Handle<HttpRequestException>()
    .Or<TaskCanceledException>()
    .OrResult<bool>(b => b == false)
    .WaitAndRetryAsync(new[] { TimeSpan.FromSeconds(10) })
    .ExecuteAsync(Bahamut.IsOperationalAsync);

Log.Verbose("安裝對應版本的 Chrome driver");
new DriverManager().SetUpDriver(new ChromeConfig());

const string bahaName = "巴哈姆特電玩資訊站{Status}";
if (await operatingTask)
{
    Log.Information(bahaName, "運作正常");
}
else
{
    Log.Fatal(bahaName, "壞了");
    Log.CloseAndFlush();
    return;
}

try
{
    using ChromeDriver driver = new();
    Log.Debug("{Driver} created", driver.GetType().Name);

    string username = config.GetValue<string>("BAHAMUT_USERNAME");
    string password = config.GetValue<string>("BAHAMUT_PASSWORD");
    if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
    {
        throw new InvalidOperationException("Login credentials is missing");
    }

    Log.Debug("Got login credentials");

    LoginPage loginPage = new(driver);
    var homePage = loginPage.LogIn(username, password);

    if (homePage.IsLoggedIn())
    {
        Log.Information("Logged in successfully");
    }
    else
    {
        Log.Fatal("Log in failed");
        return;
    }

    //Log.Information("Sign in? {SignInResult}", await Bahamut.IsSignedInAsync(null!));

    homePage.GetDoubleDailySignInReward();
    //Log.Information("Sign in? {SignInResult}", await Bahamut.IsSignedInAsync(null!));
}
catch (WebDriverException wdEx)
{
    Log.Error(wdEx, "Web driver error");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Unexpected error occured");
}
finally
{
    Log.CloseAndFlush();
}
