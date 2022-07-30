using System.ComponentModel;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Polly;
using Serilog;

namespace WebBrowserAutomation.Components;

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
    private readonly By _videoBoxCountDownBy = By.Id("count_down");

    /// <summary>
    /// 關閉影片按鈕。
    /// </summary>
    /// <remarks>
    /// &lt;div id="close_button_icon"&gt;&lt;/div&gt;
    /// </remarks>
    private readonly By _closeVideoBoxDivBy = By.Id("close_button_icon");

    /// <summary>
    /// 開始播放有聲影片前的確認。
    /// </summary>
    /// <remarks>
    /// &lt;div class=&quot;rewardResumebutton&quot;&gt;&lt;/div&gt;
    /// </remarks>
    private readonly By _resumeAdDivBy =
        By.CssSelector("div.videoAdUi > div.rewardDialogueWrapper[style=\"\"] div.rewardResumebutton");

    /// <summary>
    /// 滿版廣告影片的倒數時間<c>div</c>
    /// </summary>
    /// <remarks>
    /// &lt;div class="rewardedAdUiAttribution"&gt;獎勵將於 5 秒後發放&lt;/div&gt;
    /// </remarks>
    private readonly By _fullFrameAdCountDownBy = By.CssSelector("div.rewardedAdUiAttribution");

    /// <summary>
    /// 關閉滿版影片按鈕。
    /// </summary>
    /// <remarks>
    /// &lt;img src="https://googleads.g.doubleclick.net/pagead/images/gmob/close-circle-30x30.png"&gt;
    /// </remarks>
    private readonly By _closeFullFrameAdImgBy = By.CssSelector(
        "div.ad-video > img[src=\"https://googleads.g.doubleclick.net/pagead/images/gmob/close-circle-30x30.png\"]");

    private readonly IWebDriver _driver;

    public GoogleAdIframe(IWebDriver driver)
    {
        _driver = driver;
    }

    /// <summary>
    /// Check the ad type, get remaining time and close the ad when it finished.
    /// </summary>
    /// <exception cref="InvalidEnumArgumentException">An unknown type of ad.</exception>
    public void WatchAdThenCloseIt()
    {
        IWebElement countDownDiv;
        var adType = CheckAdType();
        Log.Debug("Ad type: {AdType}", adType);
        WebDriverWait wait = new(_driver, TimeSpan.FromSeconds(Global.SeleniumOptions.ExplicitWaitInSec))
        {
            PollingInterval = TimeSpan.FromMilliseconds(Global.SeleniumOptions.PollingIntervalInMs)
        };
        wait.IgnoreExceptionTypes(typeof(NoSuchElementException));

        switch (adType)
        {
            case AdType.FullFrame:
                wait.Until(drv => drv.FindElement(_resumeAdDivBy)).Click();
                countDownDiv = _driver.FindElement(_fullFrameAdCountDownBy);
                break;
            case AdType.VideoBox:
                countDownDiv = _driver.FindElement(_videoBoxCountDownBy);
                break;
            case AdType.Unknown:
            default:
                throw new InvalidEnumArgumentException("Unknown ad type");
        }

        // Wait the text of the count down div to be displayed.
        Policy
            .HandleResult<bool>(b => b)
            .WaitAndRetry(new[] { TimeSpan.FromMilliseconds(300), TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3) })
            .Execute(() => string.IsNullOrEmpty(countDownDiv.Text));

        Log.Debug("Count down text: {CountDownInfo}", countDownDiv.Text);
        int remainingSeconds = ParseCountDownSeconds(countDownDiv.Text);
        Log.Information("廣告剩餘 {RemainingSeconds} 秒", remainingSeconds);
        const int extraSec = 1;
        Task delay = Task.Delay(TimeSpan.FromSeconds(remainingSeconds + extraSec));

#pragma warning disable CS8509
        IWebElement closeIcon = adType switch
#pragma warning restore CS8509
        {
            AdType.FullFrame => _driver.FindElement(_closeFullFrameAdImgBy),
            AdType.VideoBox => _driver.FindElement(_closeVideoBoxDivBy)
        };

        // Block this thread until the ad is finished and closed.
        delay.ContinueWith(_ => closeIcon.Click()).Wait();
    }

    private static int ParseCountDownSeconds(string? countDown)
    {
        if (countDown != null)
        {
            for (int i = 0; i < countDown.Length; i++)
            {
                if (!char.IsDigit(countDown[i]))
                {
                    continue;
                }

                int endIdx = countDown.IndexOf(' ', i + 1);
                return int.Parse(countDown.Substring(i, endIdx - i));
            }
        }

        const int defaultSeconds = 40;
        return defaultSeconds;
    }

    private AdType CheckAdType()
    {
        if (_driver.FindElements(By.Id("mys-wrapper")).Count == 1)
        {
            return AdType.VideoBox;
        }

        if (_driver.FindElements(By.Id("google-rewarded-video")).Count == 1)
        {
            return AdType.FullFrame;
        }

        return AdType.Unknown;
    }

    enum AdType
    {
        VideoBox,
        FullFrame,
        Unknown
    }
}
