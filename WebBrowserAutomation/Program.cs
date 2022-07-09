using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using WebBrowserAutomation;
using WebBrowserAutomation.Pages;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

using IHost host = Host.CreateDefaultBuilder().Build();
IConfiguration config = host.Services.GetRequiredService<IConfiguration>();

var task = Bahamut.IsOperationalAsync();

new DriverManager().SetUpDriver(new ChromeConfig());
ChromeOptions options = new();

Console.Write("巴哈姆特電玩資訊站");
if (await task)
{
    Console.WriteLine("運作正常");
}
else
{
    Console.WriteLine("壞了。自動結束!");
    return;
}

WebDriver driver = null!;
try
{
    driver = new ChromeDriver(options);
    driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(500);

    string username = config.GetValue<string>("BAHAMUT_USERNAME");
    string password = config.GetValue<string>("BAHAMUT_PASSWORD");
    if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
    {
        throw new InvalidOperationException("Login credentials is missing");
    }

    LoginPage loginPage = new(driver);
    var homePage = loginPage.LogInValidUser(username, password);

    Console.WriteLine($"Logged in? {homePage.IsLoggedIn()}");

    homePage.GetDoubleDailySignInGift();
}
finally
{
    driver.Quit();
}
