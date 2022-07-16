using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Serilog;

namespace WebBrowserAutomation.Pages;

public class LoginPage
{
    public const string Url = "https://user.gamer.com.tw/login.php";

    // <form id="form-login" method="post">
    private readonly By _loginFormBy = By.Id("form-login");

    // <input name="userid" type="text">
    private readonly By _userIdBy = By.Name("userid");

    // <input name="password" type="password">
    private readonly By _passwordBy = By.Name("password");

    // <a id="btn-login" href="###">
    private readonly By _loginBy = By.Id("btn-login");
    private readonly IWebDriver _driver;

    public LoginPage(IWebDriver driver)
    {
        _driver = driver;
    }

    /// <summary>
    /// Log in as a valid user.
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <exception cref="Exception">Cannot find a login form.</exception>
    public HomePage LogIn(string username, string password)
    {
        // ReSharper disable once SuspiciousTypeConversion.Global
        if (!new Uri(Url).Equals(_driver.Url))
        {
            Log.Debug("Current URL {Url} does not match {LoginUrl}", _driver.Url, Url);
            _driver.Navigate().GoToUrl(Url);
        }

        var loginForm = _driver.FindElement(_loginFormBy);
        if (loginForm == null)
        {
            throw new Exception("Cannot find the login form.");
        }

        Log.Verbose("Username: {Username}, Password: {Password}", username, password);
        loginForm.FindElement(_userIdBy).SendKeys(username);
        loginForm.FindElement(_passwordBy).SendKeys(password);
        loginForm.FindElement(_loginBy).Click();

        WebDriverWait wait = new(_driver, TimeSpan.FromSeconds(3)) { PollingInterval = TimeSpan.FromMilliseconds(500) };
        Uri homeUri = new(HomePage.Url);
        bool result;
        try
        {
            result = wait.Until(e => homeUri.Equals(e.Url));
        }
        catch (WebDriverException wdEx)
        {
            if (wdEx.Message.Contains("unknown error: unexpected command response"))
            {
                result = false;
                Log.Warning(wdEx, "Chrome version 103 bug");
            }
            else
            {
                throw;
            }
        }

        Log.Information("Login? {Result}", result);

        return new HomePage(_driver);
    }
}
