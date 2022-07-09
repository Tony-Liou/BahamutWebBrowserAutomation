using OpenQA.Selenium;

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
    /// <div id="count_down">12 秒後可獲獎勵</div>
    /// OR
    /// <div id="count_down" style="visibility: hidden;">0 秒後可獲獎勵</div>
    /// </example>
    private static readonly By CountDownBy = By.Id("count_down");

    // <div id="close_button_icon"></div>
    private static readonly By CloseButtonIconBy = By.Id("close_button_icon");

    private readonly By _resumeAdDivBy =
        By.CssSelector("div.videoAdUi > div.rewardDialogueWrapper[style=\"\"] div.rewardResumebutton");

    private readonly By _closeAdImgBy = By.CssSelector(
        "div.ad-video > img[src=\"https://googleads.g.doubleclick.net/pagead/images/gmob/close-circle-30x30.png\"]");

    private readonly WebDriver _driver;

    public GoogleAdIframe(WebDriver driver)
    {
        _driver = driver;
    }

    public void WatchAdThenCloseIt()
    {
        _driver.FindElement(_resumeAdDivBy).Click();

        string countDownInfo = _driver.FindElement(CountDownBy).Text;
        int remainingSeconds = int.Parse(countDownInfo[..countDownInfo.IndexOf(' ')]);

        // Add an extra second as buffer.
        Task delay = Task.Delay(TimeSpan.FromSeconds(remainingSeconds + 1));

        var crossIcon = _driver.FindElement(CloseButtonIconBy);

        // Block this thread until the ad is finished and closed.
        delay.ContinueWith(_ => crossIcon.Click()).Wait();
    }
}
