using OpenQA.Selenium;
using Serilog;

namespace WebBrowserAutomation.Components;

public class ReCaptchaIframe
{
    private const string Url = "https://www.google.com/recaptcha/enterprise/anchor";
    private const string ImageSelectionUrl = "https://www.google.com/recaptcha/enterprise/bframe";
    private const int SleepTimeInMilliseconds = 1000;

    //private readonly By _mainDivBy = By.Id("rc-anchor-container");

    /// <summary>
    /// reCAPTCHA checkbox.
    /// </summary>
    private readonly By _checkboxBy = By.Id("recaptcha-anchor");

    private readonly By _imageSelectionBy = By.Id("rc-imageselect");

    private readonly By _audioButtonBy = By.Id("recaptcha-audio-button");

    // https://www.google.com/recaptcha/enterprise/payload/audio.mp3
    private readonly By _audioDownloadLinkBy = By.CssSelector("a.rc-audiochallenge-tdownload-link");

    private readonly IWebDriver _driver;

    public ReCaptchaIframe(IWebDriver driver)
    {
        _driver = driver;
    }

    public void Solve()
    {
        var checkbox = _driver.FindElement(_checkboxBy);
        Log.Information("Checkbox found. Sleeping for {SleepTimeInMilliseconds} milliseconds before clicking",
            SleepTimeInMilliseconds);
        Thread.Sleep(SleepTimeInMilliseconds);
        checkbox.Click();
    }
}
