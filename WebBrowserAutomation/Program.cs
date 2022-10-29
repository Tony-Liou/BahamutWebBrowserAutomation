using System.Net;
using System.Runtime.InteropServices;
using System.Web;
using AutoMapper;
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

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((hostingContext, configuration) =>
    {
        var env = hostingContext.HostingEnvironment;

        // Override the default configuration provider with the required json file
        configuration.AddJsonFile("appsettings.json", true);
        configuration.AddJsonFile($"appsettings.{env.EnvironmentName}.json");
    })
    .Build();
var config = host.Services.GetRequiredService<IConfiguration>();
var environment = host.Services.GetRequiredService<IHostEnvironment>();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(config)
    .CreateLogger();

var mapperConfig = new MapperConfiguration(cfg =>
{
    cfg.CreateMap<OpenQA.Selenium.Cookie, Cookie>().ConvertUsing(c =>
        new Cookie(c.Name, HttpUtility.UrlEncode(c.Value), c.Path, c.Domain)
        {
            Secure = c.Secure, HttpOnly = c.IsHttpOnly, Expires = c.Expiry ?? DateTime.MinValue
        });
});
var mapper = mapperConfig.CreateMapper();

try
{
    Log.Debug("Start configuring");

    LogRuntimeEnvironment(environment);

    Log.Verbose("正在測試巴哈網站是否正常");
    var operatingTask = Policy
        .Handle<HttpRequestException>()
        .Or<TaskCanceledException>()
        .OrResult<bool>(b => b == false)
        .WaitAndRetryAsync(new[] { TimeSpan.FromSeconds(10) }, (_, timeSpan) =>
        {
            Log.Warning("巴哈網站無法連線，等待 {TimeSpan} 秒後重試", timeSpan.TotalSeconds);
        })
        .ExecuteAsync(Bahamut.IsOperationalAsync);

    Log.Verbose("Installing Chrome driver");
    string driverLocation = Policy
        .Handle<WebException>()
        .WaitAndRetry(new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10) }, ((_, timeSpan) =>
        {
            Log.Warning("下載 Chrome driver 失敗，等待 {TimeSpan} 秒後重試", timeSpan.TotalSeconds);
        }))
        .Execute(() => new DriverManager().SetUpDriver(new ChromeConfig(), VersionResolveStrategy.MatchingBrowser));
    Log.Verbose("Web browser driver's location: {DriverLocation}", driverLocation);

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

    OpenQA.Selenium.Chrome.ChromeOptions options = new();
    ChromeDriverService service = ChromeDriverService.CreateDefaultService();
    var chromeConfig = config.GetSection(ChromeOptions.ConfigurationSectionName).Get<ChromeOptions>();
    if (chromeConfig != null)
    {
        options.AddArguments(chromeConfig.Arguments ?? Array.Empty<string>());
        service.SuppressInitialDiagnosticInformation = chromeConfig.SuppressInitialDiagnosticInformation;
        service.HideCommandPromptWindow = chromeConfig.HideCommandPromptWindow;
    }

    #endregion

    Global.SeleniumOptions = config.GetSection(SeleniumOptions.ConfigurationSectionName).Get<SeleniumOptions>();

    using ChromeDriver driver = new(service, options);
    try
    {
        #region Main process

        Log.Debug("{Driver} created", driver.GetType().Name);
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(Global.SeleniumOptions.ImplicitWaitInSec);

        string username = config.GetValue<string>("BAHAMUT_USERNAME");
        string password = config.GetValue<string>("BAHAMUT_PASSWORD");
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            Log.Fatal("The login credential is missing");
            return;
        }

        Log.Verbose("Got login credentials");

        LoginPage loginPage = new HomePage(driver).ClickLoginLink();
        var homePage = loginPage.LogIn(username, password);

        // Wait the page to load
        await Task.Delay(1000);

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

        var browserCookies = homePage.GetAllCookies();
        var cookies = mapper.Map<IReadOnlyCollection<OpenQA.Selenium.Cookie>, List<Cookie>>(browserCookies);
        Log.Information("今日已簽到？{SignInResult}", await Bahamut.IsSignedInAsync(cookies));

        #endregion
    }
    catch (WebDriverException wdEx)
    {
        Log.Error(wdEx, "Web driver error. Current URL: {Url}", driver.Url);
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
    Log.Debug("64-bit OS: {Is64BitOS}", Environment.Is64BitOperatingSystem);
    Log.Information(".NET environment: {Environment}", hostEnv.EnvironmentName);
    Log.Debug("Current working directory: {CurrentDirectory}", hostEnv.ContentRootPath);
    Log.Debug("GC allocated approximately {Bytes} bytes", GC.GetTotalMemory(true));
}

#endregion
