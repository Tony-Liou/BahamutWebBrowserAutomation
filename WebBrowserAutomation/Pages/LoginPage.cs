using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Serilog;

namespace WebBrowserAutomation.Pages;

// ReSharper disable SuspiciousTypeConversion.Global
public class LoginPage
{
    private const string Url = "https://user.gamer.com.tw/login.php";
    private const string LoginFormId = "form-login";

    /// <summary>
    /// A login <c>form</c>.
    /// </summary>
    /// <remarks>
    /// &lt;form id="form-login" method="post"&gt;
    /// </remarks>
    private readonly By _loginFormBy = By.Id(LoginFormId);

    /// <summary>
    /// An <c>input</c> element for username.
    /// </summary>
    /// <remarks>
    /// <c>&lt;input name="userid" type="text"&gt;</c>
    /// </remarks>
    private readonly By _userIdBy = By.Name("userid");

    /// <summary>
    /// An <c>input</c> element for password.
    /// </summary>
    /// <remarks>
    /// <c>&lt;input name="password" type="password"&gt;</c>
    /// </remarks>
    private readonly By _passwordBy = By.Name("password");

    /// <summary>
    /// A submit button.
    /// </summary>
    /// <remarks>
    /// <c>&lt;a id="btn-login" href="###"&gt;</c>
    /// </remarks>
    private readonly By _loginBy = By.Id("btn-login");

    private readonly By _errorMsgBy = By.CssSelector($"#{LoginFormId}+div.caption-text.red.margin-bottom.msgdiv-alert");

    private readonly By _recaptchaBy = By.CssSelector("div.g-recaptcha iframe[title=\"reCAPTCHA\"]");

    private readonly IWebDriver _driver;

    public LoginPage(IWebDriver driver)
    {
        _driver = driver;
    }

    /// <summary>
    /// Log in as a valid user.
    /// </summary>
    /// <param name="username">巴哈姆特登入用帳號</param>
    /// <param name="password">巴哈姆特登入用密碼</param>
    /// <returns>登入後重新導向至首頁。</returns>
    public HomePage LogIn(string username, string password)
    {
        if (!Utils.PageUtils.CheckSamePage(_driver, Url))
        {
            Log.Information("Navigating to login URL");
        }

        var loginForm = _driver.FindElement(_loginFormBy);
        loginForm.FindElement(_userIdBy).SendKeys(username);
        loginForm.FindElement(_passwordBy).SendKeys(password);
        loginForm.FindElement(_loginBy).Click();

        WebDriverWait wait = new(_driver, TimeSpan.FromSeconds(Global.SeleniumOptions.ExplicitWaitInSec))
        {
            PollingInterval = TimeSpan.FromMilliseconds(Global.SeleniumOptions.PollingIntervalInMs)
        };

        var result = wait.Until(e => new Uri(HomePage.Url).Equals(e.Url));

        Log.Information("登入後重新導向至首頁：{Result}", result);

        return new HomePage(_driver);
    }
}
