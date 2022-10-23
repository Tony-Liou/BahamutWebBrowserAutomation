using OpenQA.Selenium;
using Serilog;
using WebBrowserAutomation.Components;
using WebBrowserAutomation.Pages.Utils;

namespace WebBrowserAutomation.Pages;

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

    /// <summary>
    /// The div of login form displaying error messages.
    /// </summary>
    private readonly By _errorMsgBy = By.CssSelector($"#{LoginFormId}+div.caption-text.red.margin-bottom.msgdiv-alert");

    /// <summary>
    /// The reCAPTCHA iframe.
    /// </summary>
    private readonly By _recaptchaBy = By.CssSelector("div.g-recaptcha iframe[title=\"reCAPTCHA\"]");

    private readonly IWebDriver _driver;

    public LoginPage(IWebDriver driver)
    {
        _driver = driver;

        if (!PageUtils.CheckSamePage(_driver, Url))
        {
            Log.Information("Navigating to login URL");
        }
    }

    /// <summary>
    /// Log in as a valid user.
    /// </summary>
    /// <param name="username">巴哈姆特登入用帳號</param>
    /// <param name="password">巴哈姆特登入用密碼</param>
    /// <returns>登入後重新導向至首頁。</returns>
    public HomePage LogIn(string username, string password)
    {
        Log.Verbose("Finding login form");
        var loginForm = _driver.FindElement(_loginFormBy);
        Log.Verbose("Login form found");
        loginForm.FindElement(_userIdBy).SendKeys(username);
        loginForm.FindElement(_passwordBy).SendKeys(password);
        Log.Verbose("Entered username and password");
        loginForm.FindElement(_loginBy).Click();
        Log.Information("Login form submitted");

        Log.Information("Wait 1 second...");
        Task.Delay(1000).GetAwaiter().GetResult();

        return new HomePage(_driver);
    }

    public void LoginFailHandler()
    {
        Log.Verbose("Finding login form");
        var loginForm = _driver.FindElement(_loginFormBy);

        Log.Verbose("Check the login error messages is displayed or not");
        var loginErrorDiv = loginForm.FindElement(_errorMsgBy);
        if (loginErrorDiv.Displayed)
        {
            Log.Error("巴哈登入錯誤訊息: {ErrorMessage}", loginErrorDiv.Text);
            var reCaptchaIframes = loginForm.FindElements(_recaptchaBy);
            if (reCaptchaIframes.Any())
            {
                Log.Debug("Found a reCAPTCHA iframe. Switching to it");
                _driver.SwitchTo().Frame(reCaptchaIframes[0]);
                new ReCaptchaIframe(_driver).Solve();
                _driver.SwitchTo().DefaultContent();
            }
            else
            {
                Log.Information("reCAPTCHA iframe not found");
            }
        }

        loginForm.FindElement(_loginBy).Click();
    }
}
