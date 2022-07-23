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
using Cookie = System.Net.Cookie;

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
        Log.Fatal("The login credential is missing");
        return;
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

    Log.Information("嘗試獲得雙倍簽到獎勵");
    homePage.GetDoubleDailySignInReward();
    Log.Information("已獲得雙倍簽到獎勵");

    var cookies = ConvertSeleniumCookiesToBuiltInCookies(homePage.GetAllCookies());
    Log.Information("Signed in today? {SignInResult}", await Bahamut.IsSignedInAsync(cookies));
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

static List<Cookie> ConvertSeleniumCookiesToBuiltInCookies(IReadOnlyCollection<OpenQA.Selenium.Cookie> seleniumCookies)
{
    List<Cookie> cookieList = new(seleniumCookies.Count);
    cookieList.AddRange(seleniumCookies.Select(seCookie =>
        new Cookie(seCookie.Name, seCookie.Value, seCookie.Path, seCookie.Domain)
        {
            Secure = seCookie.Secure, HttpOnly = seCookie.IsHttpOnly, Expires = seCookie.Expiry ?? DateTime.MinValue
        }));
    return cookieList;
}
