using OpenQA.Selenium;

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
    private readonly WebDriver _driver;

    public LoginPage(WebDriver driver)
    {
        _driver = driver;
    }

    /// <summary>
    /// Log in as a valid user.
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <exception cref="Exception">Cannot find a login form.</exception>
    public HomePage LogInValidUser(string username, string password)
    {
        if (_driver.Url != Url)
        {
            _driver.Navigate().GoToUrl(Url);
        }

        var loginForm = _driver.FindElement(_loginFormBy);
        if (loginForm == null)
        {
            throw new Exception("Cannot find the login form.");
        }

        loginForm.FindElement(_userIdBy).SendKeys(username);
        loginForm.FindElement(_passwordBy).SendKeys(password);
        loginForm.FindElement(_loginBy).Click();

        return new HomePage(_driver);
    }
}
