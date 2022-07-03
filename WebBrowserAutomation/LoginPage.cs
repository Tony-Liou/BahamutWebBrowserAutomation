using OpenQA.Selenium;

namespace WebBrowserAutomation;

public class LoginPage
{
    public const string Url = "https://user.gamer.com.tw/login.php";

    // <form id="form-login" method="post">
    private static readonly By loginFormBy = By.Id("form-login");
    // <input name="userid" type="text">
    private static readonly By userIdBy = By.Name("userid");
    // <input name="password" type="password">
    private static readonly By passwordBy = By.Name("password");
    // <a id="btn-login" href="###">
    private static readonly By loginBy = By.Id("btn-login");
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

        var loginForm = _driver.FindElement(loginFormBy);
        if (loginForm == null)
        {
            throw new Exception("Cannot find the login form.");
        }

        loginForm.FindElement(userIdBy).SendKeys(username);
        loginForm.FindElement(passwordBy).SendKeys(password);
        loginForm.FindElement(loginBy).Click();

        return new HomePage(_driver);
    }
}
