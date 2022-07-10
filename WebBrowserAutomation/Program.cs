using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
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

Task<bool> task = Bahamut.IsOperationalAsync();

new DriverManager().SetUpDriver(new ChromeConfig());

const string bahaName = "巴哈姆特電玩資訊站";
if (await task)
{
    Log.Debug(bahaName + "運作正常");
}
else
{
    Log.Fatal(bahaName + "壞了。自動結束!");
    return;
}

WebDriver driver = null!;
try
{
    driver = new ChromeDriver();
    //driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(500);
    Log.Debug("ChromeDriver created");

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

    homePage.GetDoubleDailySignInGift();
}
catch (Exception ex)
{
    Log.Error(ex, "Error occured");
}
finally
{
    Log.CloseAndFlush();
    driver.Quit();
}
