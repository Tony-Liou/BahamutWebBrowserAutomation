using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Serilog;

namespace WebBrowserAutomation;

/// <summary>
/// A Google ad inside an <c>iframe</c>.
/// </summary>
public class GoogleAdIframe
{
    /// <summary>
    /// A div which displays remaining time of the ad.
    /// </summary>
    /// <example>
    /// &lt;div id=&quot;count_down&quot;&gt;12 秒後可獲獎勵&lt;/div&gt;
    /// OR
    /// &lt;div id=&quot;count_down&quot; style=&quot;visibility: hidden;&quot;&gt;0 秒後可獲獎勵&lt;/div&gt;
    /// </example>
    private readonly By _countDownBy = By.Id("count_down");

    /// <summary>
    /// &lt;div id="close_button_icon"&gt;&lt;/div&gt;
    /// </summary>
    private readonly By _closeButtonIconBy = By.Id("close_button_icon");

    /// <summary>
    /// &lt;div id=&quot;close_confirmation_dialog&quot;&gt;&lt;/div&gt;
    /// </summary>
    private readonly By _closeConfirmationDialogBy = By.Id("close_confirmation_dialog");

    /// <summary>
    /// &lt;div class=&quot;rewardResumebutton&quot;&gt;&lt;/div&gt;
    /// </summary>
    /// <remarks>
    /// Before playing the ad. Full frame ad.
    /// </remarks>
    private readonly By _resumeAdDivBy =
        By.CssSelector("div.videoAdUi > div.rewardDialogueWrapper[style=\"\"] div.rewardResumebutton");

    private readonly By _fullAdCountDownBy = By.CssSelector("");

    private readonly By _closeAdImgBy = By.CssSelector(
        "div.ad-video > img[src=\"https://googleads.g.doubleclick.net/pagead/images/gmob/close-circle-30x30.png\"]");

    private readonly IWebDriver _driver;

    public GoogleAdIframe(IWebDriver driver)
    {
        _driver = driver;
    }

    public void WatchAdThenCloseIt()
    {
        WebDriverWait wait = new(_driver, TimeSpan.FromSeconds(3)) { PollingInterval = TimeSpan.FromMilliseconds(500) };
        wait.IgnoreExceptionTypes(typeof(NoSuchElementException));
        try
        {
            wait.Until(drv => drv.FindElement(_resumeAdDivBy)).Click();
        }
        catch (WebDriverTimeoutException timeoutException)
        {
            Log.Warning(timeoutException, "No resume button found");
        }

        IWebElement? countDownDiv;
        try
        {
            countDownDiv = wait.Until(drv => drv.FindElement(_countDownBy));
        }
        catch (WebDriverTimeoutException e)
        {
            string currentFrame = _driver.FindElement(By.Id("mys-content")).GetAttribute("innerHTML");
            Log.Warning(e, "No countdown div found. Current content=[{Body}]", currentFrame);
            countDownDiv = null;
        }

        int remainingSeconds;
        if (countDownDiv != null)
        {
            string countDownInfo = countDownDiv.Text;
            remainingSeconds = int.Parse(countDownInfo[..countDownInfo.IndexOf(' ')]);
        }
        else
        {
            remainingSeconds = 30;
        }
        
        const int extraSec = 1;
        Task delay = Task.Delay(TimeSpan.FromSeconds(remainingSeconds + extraSec));

        var crossIcon = _driver.FindElement(_closeButtonIconBy);

        // Block this thread until the ad is finished and closed.
        delay.ContinueWith(_ => crossIcon.Click()).Wait();
    }
}
