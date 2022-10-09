using OpenQA.Selenium;

namespace WebBrowserAutomation.Components;

public class ReCaptchaIframe
{
    private const string Url = "https://www.google.com/recaptcha/enterprise/anchor";

    /// <summary>
    /// A login <c>form</c>.
    /// </summary>
    /// <remarks>
    /// &lt;form id="form-login" method="post"&gt;
    /// </remarks>
    private readonly By _mainDivBy = By.Id("rc-anchor-container");
    
    private readonly IWebDriver _driver;
    
    public ReCaptchaIframe(IWebDriver driver)
    {
        _driver = driver;
    }
    
    public void Solve()
    {
        var iframe = _driver.FindElement(By.TagName("iframe"));
        _driver.SwitchTo().Frame(iframe);
        var mainDiv = _driver.FindElement(_mainDivBy);
        var checkbox = mainDiv.FindElement(By.Id("recaptcha-anchor"));
        checkbox.Click();
    }
}
