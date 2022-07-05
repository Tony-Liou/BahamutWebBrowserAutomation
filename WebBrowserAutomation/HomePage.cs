using OpenQA.Selenium;

namespace WebBrowserAutomation;

public class HomePage// : BaseHttpClient
{
    public const string Url = "https://www.gamer.com.tw";
    private readonly WebDriver driver;

    // <div class="TOP-my TOP-nologin">
    private readonly By avatarBy = By.ClassName("TOP-my");
    // <a id="signin-btn" onclick="Signin.showSigninMap();">
    private readonly By signinBtnBy = By.Id("signin-btn");
    // <dialog id="dialogify_{int}" class="dialogify fixed popup-dailybox" open>
    private readonly By dailyboxDialogBy = By.CssSelector("dialog.popup-dailybox");
    // <button class="popup-dailybox__btn" onclick="window.SigninAd.startAd();">
    private readonly By doubleCoinsBtnBy = By.CssSelector("button.popup-dailybox__btn");
    /**
     * <dialog id="dialogify_{int}" class="dialogify fixed" open>
     *   <form mehtod="dialog">
     *     ...
     *     <button type="submit" autofocus>確定</button>
     *     ...
     *   </form>
     * </dialog>
     */
    private readonly By confirmWatchAdBtnBy = By.CssSelector("dialog > form button[type=\"submit\"]");

    private readonly By adIframeBy = By.CssSelector("ins[data-google-query-id] iframe");

    private readonly By resumeAdDivBy =
        By.CssSelector("div.videoAdUi > div.rewardDialogueWrapper[style=\"\"] div.rewardResumebutton");

    private readonly By muteButtonBy =
        By.CssSelector("div.ad-video > img[src=\"https://www.gstatic.com/dfp/native/volume_off.png\"]");

    private readonly By closeAdImgBy = By.CssSelector(
        "div.ad-video > img[src=\"https://googleads.g.doubleclick.net/pagead/images/gmob/close-circle-30x30.png\"]");

    public HomePage(WebDriver driver)
    {
        this.driver = driver;
    }

    public bool IsLoggedIn()
    {
        var divClass = driver.FindElement(avatarBy).GetAttribute("class");
        return !divClass.Contains("TOP-nologin");
    }

    public void ClickLoginLink()
    {
        driver.FindElement(By.CssSelector("div.TOP-nologin > a:first-child")).Click();
    }

    /// <summary>
    /// Watch a 30 sec ad and then receive coins.
    /// </summary>
    public void GetDoubleDailySignInGift()
    {
        driver.FindElement(signinBtnBy).Click();

        var popupDialog = driver.FindElement(dailyboxDialogBy);
        popupDialog.FindElement(doubleCoinsBtnBy).Click();
        driver.FindElement(confirmWatchAdBtnBy).Click();

        var adIframe = driver.FindElement(adIframeBy);
        // Switch to iframe
        driver.SwitchTo().Frame(adIframe);
        
        new GoogleAdIframe(driver).WatchAdThenCloseIt();

        // Return to the top level
        driver.SwitchTo().DefaultContent();
    }
}
