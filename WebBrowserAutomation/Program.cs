using System.Net;
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
using WebDriverManager.Helpers;
using ChromeOptions = WebBrowserAutomation.Configurations.ChromeOptions;
using Cookie = System.Net.Cookie;

using IHost host = Host.CreateDefaultBuilder(args).Build();
var config = host.Services.GetRequiredService<IConfiguration>();
var environment = host.Services.GetRequiredService<IHostEnvironment>();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(config)
    .CreateLogger();

try
{
    Log.Verbose("Logging configured");

    LogRuntimeEnvironment(environment);

    Log.Verbose("正在測試巴哈網站是否正常");
    var operatingTask = Policy
        .Handle<HttpRequestException>()
        .Or<TaskCanceledException>()
        .OrResult<bool>(b => b == false)
        .WaitAndRetryAsync(new[] { TimeSpan.FromSeconds(10) })
        .ExecuteAsync(Bahamut.IsOperationalAsync);

    Log.Verbose("Installing Chrome driver");
    Policy
        .Handle<WebException>()
        .WaitAndRetry(new[] { TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30) }, (exception, timespan) =>
        {
            Log.Error(exception, "Install Chrome driver failed. Retrying after {TimeSpan}...", timespan);
        })
        .Execute(() => new DriverManager().SetUpDriver(new ChromeConfig(), VersionResolveStrategy.MatchingBrowser));

    const string bahaStatusTemplate = "巴哈姆特電玩資訊站{Status}";
    if (await operatingTask)
    {
        Log.Information(bahaStatusTemplate, "運作正常");
    }
    else
    {
        Log.Fatal(bahaStatusTemplate, "壞了88");
        return;
    }


    #region Setup the Chrome driver

    var chromeConfig = config.GetSection(nameof(ChromeOptions)).Get<ChromeOptions>();
    OpenQA.Selenium.Chrome.ChromeOptions options = new();
    options.AddArguments(chromeConfig.Arguments ?? Array.Empty<string>());
    ChromeDriverService service = ChromeDriverService.CreateDefaultService();
    service.SuppressInitialDiagnosticInformation = chromeConfig.SuppressInitialDiagnosticInformation;
    service.HideCommandPromptWindow = chromeConfig.HideCommandPromptWindow;

    #endregion

    Global.SeleniumOptions = config.GetSection(nameof(SeleniumOptions)).Get<SeleniumOptions>();

    try
    {
        #region Main process

        using ChromeDriver driver = new(service, options);
        Log.Debug("{Driver} created", driver.GetType().Name);
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(Global.SeleniumOptions.ImplicitWaitInSec);

        string username = config.GetValue<string>("BAHAMUT_USERNAME");
        string password = config.GetValue<string>("BAHAMUT_PASSWORD");
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            Log.Fatal("The login credential is missing");
            return;
        }

        Log.Debug("Got login credentials");

        LoginPage loginPage = new HomePage(driver).ClickLoginLink();
        var homePage = loginPage.LogIn(username, password);

        if (homePage.IsLoggedIn())
        {
            Log.Information("登入成功");
        }
        else
        {
            Log.Fatal("Login failed");
            return;
        }

        Log.Information("嘗試獲得雙倍簽到獎勵");
        homePage.GetDoubleDailySignInReward();
        Log.Information("獲得雙倍簽到獎勵結束");

        var cookies = ConvertSeleniumCookiesToBuiltInCookies(homePage.GetAllCookies());
        Log.Information("今日已簽到？{SignInResult}", await Bahamut.IsSignedInAsync(cookies));

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
}
finally
{
    Log.CloseAndFlush();
}

#region Local functions

// Call this function earlier for better performance.
static void LogRuntimeEnvironment(IHostEnvironment hostEnv)
{
    OSPlatform os;
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
        os = OSPlatform.Windows;
    }
    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
    {
        os = OSPlatform.Linux;
    }
    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
    {
        os = OSPlatform.OSX;
    }
    else
    {
        os = OSPlatform.FreeBSD;
    }

    Log.Debug("OS platform: {OS}", os);
    Log.Information(".NET environment: {Environment}", hostEnv.EnvironmentName);
    Log.Debug("GC allocated approximately {Bytes} bytes", GC.GetTotalMemory(true));
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
