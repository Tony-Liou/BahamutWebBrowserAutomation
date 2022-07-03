using OpenQA.Selenium;

namespace WebBrowserAutomation;

public class HomePage// : BaseHttpClient
{
    public const string Url = "https://www.gamer.com.tw";
    private readonly WebDriver _driver;

    // <div class="TOP-my TOP-nologin">
    private readonly By avatorBy = By.ClassName("TOP-my");
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

    private readonly By adIframeBy = By.CssSelector("ins[data-google-query-id] > div > iframe");

    private readonly By resumeAdDivBy =
        By.CssSelector("div.videoAdUi > div.rewardDialogueWrapper[style=\"\"] div.rewardResumebutton");

    private readonly By muteButtonBy =
        By.CssSelector("div.ad-video > img[src=\"https://www.gstatic.com/dfp/native/volume_off.png\"]");

    private readonly By closeAdImgBy = By.CssSelector(
        "div.ad-video > img[src=\"https://googleads.g.doubleclick.net/pagead/images/gmob/close-circle-30x30.png\"]");

    public HomePage(WebDriver driver)
    {
        _driver = driver;
    }

    public bool IsLoggedIn()
    {
        var divClass = _driver.FindElement(avatorBy).GetAttribute("class");
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
        _driver.FindElement(signinBtnBy).Click();

        var popupDialog = _driver.FindElement(dailyboxDialogBy);
        popupDialog.FindElement(doubleCoinsBtnBy).Click();
        _driver.FindElement(confirmWatchAdBtnBy).Click();

        var adIframe = _driver.FindElement(adIframeBy);
        // Switch to iframe
        _driver.SwitchTo().Frame(adIframe);
        _driver.FindElement(resumeAdDivBy).Click();

        // TODO: wait the ad finishing
        Thread.Sleep(30 * 1000);

        _driver.FindElement(closeAdImgBy).Click();

        // Return to the top level
        _driver.SwitchTo().DefaultContent();
    }
}
