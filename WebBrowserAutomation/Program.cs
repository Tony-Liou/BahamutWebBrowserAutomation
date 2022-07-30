using System.Runtime.InteropServices;
using System.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Polly;
using Serilog;
using WebBrowserAutomation;
using WebBrowserAutomation.Configurations;
using WebBrowserAutomation.Pages;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;
using ChromeOptions = WebBrowserAutomation.Configurations.ChromeOptions;
using Cookie = System.Net.Cookie;

using IHost host = Host.CreateDefaultBuilder(args).Build();
var config = host.Services.GetRequiredService<IConfiguration>();
var environment = host.Services.GetRequiredService<IHostEnvironment>();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(config)
    .CreateLogger();

#region Log debug info

Log.Debug(".NET environment: {Environment}", environment.EnvironmentName);

OSPlatform osPlatform;
if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    osPlatform = OSPlatform.Windows;
}
else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
{
    osPlatform = OSPlatform.Linux;
}
else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
{
    osPlatform = OSPlatform.OSX;
}
else
{
    osPlatform = OSPlatform.FreeBSD;
}

Log.Debug("OS platform: {OS}", osPlatform);

#endregion

#region Determine the target website's status

Log.Verbose("正在測試巴哈是否正常");
var operatingTask = Policy
    .Handle<HttpRequestException>()
    .Or<TaskCanceledException>()
    .OrResult<bool>(b => b == false)
    .WaitAndRetryAsync(new[] { TimeSpan.FromSeconds(10) })
    .ExecuteAsync(Bahamut.IsOperationalAsync);

Log.Verbose("安裝對應版本的 Chrome driver");
new DriverManager().SetUpDriver(new ChromeConfig());

const string bahaStatusTemplate = "巴哈姆特電玩資訊站{Status}";
if (await operatingTask)
{
    Log.Information(bahaStatusTemplate, "運作正常");
}
else
{
    Log.Fatal(bahaStatusTemplate, "壞了");
    Log.CloseAndFlush();
    return;
}

#endregion

var chromeConfig = config.GetSection(nameof(ChromeOptions)).Get<ChromeOptions>();
OpenQA.Selenium.Chrome.ChromeOptions options = new();
options.AddArguments(chromeConfig.Arguments ?? Array.Empty<string>());
ChromeDriverService service = ChromeDriverService.CreateDefaultService();
service.SuppressInitialDiagnosticInformation = chromeConfig.SuppressInitialDiagnosticInformation;
service.HideCommandPromptWindow = chromeConfig.HideCommandPromptWindow;

try
{
    #region Main process

    using ChromeDriver driver = new(service, options);
    Log.Debug("{Driver} created", driver.GetType().Name);
    AssignGlobalConfig(config);
    driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(Global.SeleniumOptions.ImplicitWaitInSec);

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
    Log.Information("獲得雙倍簽到獎勵結束");

    var cookies = ConvertSeleniumCookiesToBuiltInCookies(homePage.GetAllCookies());
    Log.Information("Signed in today? {SignInResult}", await Bahamut.IsSignedInAsync(cookies));

    #endregion
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

#region Local functions

static void AssignGlobalConfig(IConfiguration config)
{
    Global.SeleniumOptions = config.GetSection(nameof(SeleniumOptions)).Get<SeleniumOptions>();
}

static List<Cookie> ConvertSeleniumCookiesToBuiltInCookies(IReadOnlyCollection<OpenQA.Selenium.Cookie> seleniumCookies)
{
    List<Cookie> cookieList = new(seleniumCookies.Count);
    cookieList.AddRange(seleniumCookies.Select(seCookie =>
        new Cookie(seCookie.Name, HttpUtility.UrlEncode(seCookie.Value), seCookie.Path, seCookie.Domain)
        {
            Secure = seCookie.Secure, HttpOnly = seCookie.IsHttpOnly, Expires = seCookie.Expiry ?? DateTime.MinValue
        }));
    return cookieList;
}

#endregion
