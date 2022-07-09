using OpenQA.Selenium;

namespace WebBrowserAutomation.Pages;

public class HomePage// : BaseHttpClient
{
    public const string Url = "https://www.gamer.com.tw";
    private readonly WebDriver _driver;

    // <div class="TOP-my TOP-nologin">
    private readonly By _avatarBy = By.ClassName("TOP-my");
    // <a id="signin-btn" onclick="Signin.showSigninMap();">
    private readonly By _signinBtnBy = By.Id("signin-btn");
    // <dialog id="dialogify_{int}" class="dialogify fixed popup-dailybox" open>
    private readonly By _dailyboxDialogBy = By.CssSelector("dialog.popup-dailybox");
    // <button class="popup-dailybox__btn" onclick="window.SigninAd.startAd();">
    private readonly By _doubleCoinsBtnBy = By.CssSelector("button.popup-dailybox__btn");
    /**
     * <dialog id="dialogify_{int}" class="dialogify fixed" open>
     *   <form mehtod="dialog">
     *     ...
     *     <button type="submit" autofocus>確定</button>
     *     ...
     *   </form>
     * </dialog>
     */
    private readonly By _confirmWatchAdBtnBy = By.CssSelector("dialog > form button[type=\"submit\"]");

    private readonly By _adIframeBy = By.CssSelector("ins[data-google-query-id] iframe");

    private readonly By _resumeAdDivBy =
        By.CssSelector("div.videoAdUi > div.rewardDialogueWrapper[style=\"\"] div.rewardResumebutton");

    private readonly By _muteButtonBy =
        By.CssSelector("div.ad-video > img[src=\"https://www.gstatic.com/dfp/native/volume_off.png\"]");

    private readonly By _closeAdImgBy = By.CssSelector(
        "div.ad-video > img[src=\"https://googleads.g.doubleclick.net/pagead/images/gmob/close-circle-30x30.png\"]");

    public HomePage(WebDriver driver)
    {
        if (new Uri(Url).Equals(driver.Url))
        {
            this._driver = driver; 
        }
        else
        {
            throw new InvalidOperationException("Not in the home page.");
        }
    }

    public bool IsLoggedIn()
    {
        var divClass = _driver.FindElement(_avatarBy).GetAttribute("class");
        return !divClass.Contains("TOP-nologin");
    }

    public void ClickLoginLink()
    {
        _driver.FindElement(By.CssSelector("div.TOP-nologin > a:first-child")).Click();
    }

    /// <summary>
    /// Watch a 30 sec ad and then receive coins.
    /// </summary>
    public void GetDoubleDailySignInGift()
    {
        _driver.FindElement(_signinBtnBy).Click();

        var popupDialog = _driver.FindElement(_dailyboxDialogBy);
        popupDialog.FindElement(_doubleCoinsBtnBy).Click();
        _driver.FindElement(_confirmWatchAdBtnBy).Click();

        var adIframe = _driver.FindElement(_adIframeBy);
        // Switch to iframe
        _driver.SwitchTo().Frame(adIframe);
        
        new GoogleAdIframe(_driver).WatchAdThenCloseIt();

        // Return to the top level
        _driver.SwitchTo().DefaultContent();
    }
}
