using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using Serilog;

namespace WebBrowserAutomation.Pages;

public class HomePage
{
    public const string Url = "https://www.gamer.com.tw/";

    private readonly IWebDriver _driver;

    // <div class="TOP-my TOP-nologin">
    private readonly By _avatarBy = By.ClassName("TOP-my");

    // <a id="signin-btn" onclick="Signin.showSigninMap();">
    private readonly By _signinBtnBy = By.Id("signin-btn");

    // <dialog id="dialogify_{int}" class="dialogify fixed popup-dailybox" open>
    private readonly By _dailyboxDialogBy = By.CssSelector("dialog.popup-dailybox");

    // <button class="popup-dailybox__btn" onclick="window.SigninAd.startAd();">
    private readonly By _doubleCoinsBtnBy = By.CssSelector("button.popup-dailybox__btn");

    /**
     * <dialog id="dialogify_{int}" class="dialogify fixed">
     *   <form mehtod="dialog">
     *     ...
     *     <button type="submit">確定</button>
     *     ...
     *   </form>
     * </dialog>
     */
    private readonly By _confirmWatchAdBtnBy = By.CssSelector("dialog > form button[type=\"submit\"]");

    /// <summary>
    /// &lt;ins id="gpt_unit_/1017768/AD_mobileweb_signin_videorewarded_2" data-google-query-id="CI_W-6_L_PgCFQ7OfAod6sYPFA"&gt;
    /// &lt;div id="google_ads_iframe_/1017768/AD_mobileweb_signin_videorewarded_2__container__"&gt;
    /// &lt;iframe src="" id="google_ads_iframe_/1017768/AD_mobileweb_signin_videorewarded_2"&gt;
    /// </summary>
    private readonly By _adIframeBy = By.CssSelector("ins[data-google-query-id] iframe");

    private readonly By _resumeAdDivBy =
        By.CssSelector("div.videoAdUi > div.rewardDialogueWrapper[style=\"\"] div.rewardResumebutton");

    private readonly By _muteButtonBy =
        By.CssSelector("div.ad-video > img[src=\"https://www.gstatic.com/dfp/native/volume_off.png\"]");

    private readonly By _closeAdImgBy = By.CssSelector(
        "div.ad-video > img[src=\"https://googleads.g.doubleclick.net/pagead/images/gmob/close-circle-30x30.png\"]");

    public HomePage(IWebDriver driver)
    {
        // ReSharper disable once SuspiciousTypeConversion.Global
        if (!new Uri(Url).Equals(driver.Url))
        {
            Log.Fatal("Current url {Url} is not {HomeUrl}", driver.Url, Url);
        }

        _driver = driver;
    }

    public bool IsLoggedIn()
    {
        Log.Verbose("Checking the personal avatar is existent");

        var divClass = _driver.FindElement(_avatarBy).GetAttribute("class");
        return !divClass.Contains("TOP-nologin");
    }

    public void ClickLoginLink()
    {
        _driver.FindElement(By.CssSelector("div.TOP-nologin > a:first-child")).Click();
    }

    /// <summary>
    /// Watch a 30 seconds ad and then receive a reward.
    /// </summary>
    public void GetDoubleDailySignInGift()
    {
        Log.Verbose("Getting double daily sign in reward");
        WebDriverWait wait = new(_driver, TimeSpan.FromSeconds(3)) { PollingInterval = TimeSpan.FromMilliseconds(500) };
        wait.IgnoreExceptionTypes(typeof(ElementClickInterceptedException));
        var signinBtn = wait.Until(ExpectedConditions.ElementToBeClickable(_signinBtnBy));
        signinBtn.Click();

        var popUpDialog = _driver.FindElement(_dailyboxDialogBy);
        popUpDialog.FindElement(_doubleCoinsBtnBy).Click();
        wait.IgnoreExceptionTypes(typeof(NoSuchElementException));
        wait.Until(drv => drv.FindElement(_confirmWatchAdBtnBy)).Click();

        var adIframe = _driver.FindElement(_adIframeBy);
        Log.Debug("Switching to the ad iframe");
        _driver.SwitchTo().Frame(adIframe);

        new GoogleAdIframe(_driver).WatchAdThenCloseIt();

        Log.Debug("Returning to the top level");
        _driver.SwitchTo().DefaultContent();
    }
}
