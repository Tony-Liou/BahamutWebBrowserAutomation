using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using WebBrowserAutomation;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

Console.WriteLine("Starting...");

var task = Bahamut.IsOperationalAsync();

new DriverManager().SetUpDriver(new ChromeConfig());
var options = new ChromeOptions();

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

    var (username, password) = GetCredentials();
    if (username == null || password == null)
    {
        throw new InvalidOperationException("Login credentials is missing");
    }

    LoginPage loginPage = new(driver);
    var homePage = loginPage.LogInValidUser(username, password);
    Console.WriteLine($"Logged in? {homePage.IsLoggedIn()}");
}
finally
{
    driver.Quit();
}

static (string? user, string? pwd) GetCredentials()
{
    const string userKey = "BAHA_USER";
    const string pwdKey = "BAHA_PWD";
    var user = Environment.GetEnvironmentVariable(userKey);
    var pwd = Environment.GetEnvironmentVariable(pwdKey);

    return (user, pwd);
}
