using System.Collections.ObjectModel;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Polly;
using SeleniumExtras.WaitHelpers;
using Serilog;
using WebBrowserAutomation.Components;
using WebBrowserAutomation.Pages.Utils;

namespace WebBrowserAutomation.Pages;

public class HomePage
{
    public const string Url = "https://www.gamer.com.tw/";

    private const string NotLoggedInClassName = "TOP-nologin";

    /// <summary>
    /// 右上角的登入按鈕。
    /// </summary>
    private readonly By _loginLinkBy = By.CssSelector($"div.{NotLoggedInClassName} > a:first-child");

    /// <summary>
    /// 右上角個人資訊區塊。
    /// </summary>
    /// <remarks>
    /// &lt;div class="TOP-my TOP-nologin"&gt;
    /// </remarks>
    private readonly By _avatarBy = By.ClassName("TOP-my");

    /// <summary>
    /// 每日簽到按鈕。
    /// </summary>
    /// <remarks>
    /// &lt;a id="signin-btn" onclick="Signin.showSigninMap();"&gt;
    /// </remarks>
    private readonly By _signinBtnBy = By.Id("signin-btn");

    /// <summary>
    /// 每日簽到月曆。(顯示已簽到天數的儀表板)
    /// </summary>
    // <dialog id="dialogify_{int}" class="dialogify fixed popup-dailybox" open>
    private readonly By _dailyBoxDialogBy = By.CssSelector("dialog.popup-dailybox");

    /// <summary>
    /// 領取雙倍巴幣按鈕。
    /// </summary>
    /// <remarks>
    /// &lt;button class="popup-dailybox__btn" onclick="window.SigninAd.startAd();"&gt;
    /// </remarks>
    private readonly By _doubleCoinsBtnBy = By.CssSelector("button.popup-dailybox__btn");

    /// <summary>
    /// 確認要觀看廣告的按鈕。
    /// </summary>
    private readonly By _confirmWatchAdBtnBy = By.CssSelector("dialog > form button[type=\"submit\"]");

    /// <summary>
    /// 新加入的廣告 <c>iframe</c>。
    /// </summary>
    /// <remarks>
    /// &lt;iframe src="" id="google_ads_iframe_/1017768/AD_mobileweb_signin_videorewarded_2"&gt;
    /// </remarks>
    private readonly By _adIframeBy = By.CssSelector("ins[data-google-query-id] iframe");

    private readonly IWebDriver _driver;

    public HomePage(IWebDriver driver)
    {
        _driver = driver;
        PageUtils.CheckSamePage(_driver, Url);

        HandleCurrentPage();
    }

    public bool IsAtHomePage() =>
        _driver.FindElements(By.CssSelector("body > div.BA-topbg > div.BA-wrapper.BA-top")).Count != 0;

    /// <summary>
    /// Check the top right corner of the page to find whether a personal avatar is displayed.
    /// </summary>
    /// <returns><c>true</c> if found the avatar; otherwise, <c>false</c>.</returns>
    public bool IsLoggedIn()
    {
        Log.Verbose("Checking the personal avatar is existent");

        var divClass = _driver.FindElement(_avatarBy).GetAttribute("class");
        Log.Debug("Class of the personal avatar: {DivClass}", divClass);
        return !divClass.Contains(NotLoggedInClassName) || IsDailyBoxDialogOpened();
    }

    public bool IsDailyBoxDialogOpened()
    {
        Log.Verbose("Checking the daily box dialog is opened");

        var dialogs = _driver.FindElements(_dailyBoxDialogBy);
        return dialogs.Count == 1;
    }

    public LoginPage ClickLoginLink()
    {
        WebDriverWait wait = new(_driver, TimeSpan.FromSeconds(Global.SeleniumOptions.ExplicitWaitInSec))
        {
            PollingInterval = TimeSpan.FromMilliseconds(Global.SeleniumOptions.PollingIntervalInMs)
        };
        var link = wait.Until(d => d.FindElement(_loginLinkBy));
        link.Click();

        return new LoginPage(_driver);
    }

    /// <summary>
    /// Watch an ad and then receive a reward.
    /// </summary>
    public HomePage GetDoubleDailySignInReward()
    {
        Log.Verbose("Getting double daily sign in reward");

        WebDriverWait wait = new(_driver, TimeSpan.FromSeconds(Global.SeleniumOptions.ExplicitWaitInSec))
        {
            PollingInterval = TimeSpan.FromMilliseconds(Global.SeleniumOptions.PollingIntervalInMs)
        };
        IWebElement popUpDialog;

        var dailyBoxList = _driver.FindElements(_dailyBoxDialogBy);
        if (dailyBoxList.Count == 0)
        {
            Log.Debug("Daily sign in dialog is not found");

            var signinBtn = wait.Until(ExpectedConditions.ElementToBeClickable(_signinBtnBy));
            try
            {
                signinBtn.Click();
                Log.Verbose("Clicked the signin button");
            }
            catch (ElementClickInterceptedException clickEx)
            {
                Log.Warning(clickEx, "The signin button is not clickable. Execute JS instead");
                ((IJavaScriptExecutor)_driver).ExecuteScript("Signin.showSigninMap();");
            }

            popUpDialog = _driver.FindElement(_dailyBoxDialogBy);
        }
        else
        {
            popUpDialog = dailyBoxList[0];
        }

        var doubleCoinsButton = popUpDialog.FindElement(_doubleCoinsBtnBy);
        if (doubleCoinsButton.Enabled)
        {
            wait.Until(ExpectedConditions.ElementToBeClickable(doubleCoinsButton));
            try
            {
                doubleCoinsButton.Click();
                Log.Verbose("Clicked the watch ad button");
            }
            catch (ElementClickInterceptedException clickEx)
            {
                Log.Warning(clickEx, "The watch ad button is not clickable. Execute JS instead");
                ((IJavaScriptExecutor)_driver).ExecuteScript("window.SigninAd.startAd();");
            }
        }
        else
        {
            Log.Information("領取雙倍巴幣按鈕已停用，今日已領取？");
            return this;
        }

        wait.IgnoreExceptionTypes(typeof(NoSuchElementException));
        var confirmationBtn = Policy
            .Handle<WebDriverTimeoutException>()
            .Retry()
            .Execute(() => wait.Until(drv => drv.FindElement(_confirmWatchAdBtnBy)));
        confirmationBtn.Click();
        Log.Verbose("Clicked the confirmation button");

        var adIframe = _driver.FindElement(_adIframeBy);
        Log.Verbose("Switching to the ad iframe");
        _driver.SwitchTo().Frame(adIframe);

        new GoogleAdIframe(_driver).WatchAdThenCloseIt();

        Log.Verbose("Returning to the top level");
        _driver.SwitchTo().DefaultContent();

        return this;
    }

    public ReadOnlyCollection<Cookie> GetAllCookies() => _driver.Manage().Cookies.AllCookies;

    private void HandleCurrentPage()
    {
        switch (_driver.Url)
        {
            case Url:
                break;
            case SpecialEventPage.Url:
                Log.Information("偵測到特別活動 URL，判斷中...");
                if (IsAtHomePage())
                {
                    Log.Information("在首頁，繼續執行");
                }
                else
                {
                    Log.Information("嘗試重新導向到首頁");
                    new SpecialEventPage(_driver).ClickGoToHomePage();
                }
                break;
            default:
                Log.Warning("未知網址：{Url}", _driver.Url);
                break;
        }
    }
}
