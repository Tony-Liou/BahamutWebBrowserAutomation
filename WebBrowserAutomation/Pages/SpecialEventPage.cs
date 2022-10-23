using OpenQA.Selenium;

namespace WebBrowserAutomation.Pages;

/// <summary>
/// If there is an special event page before entering the home page.
/// </summary>
class SpecialEventPage
{
    public const string Url = "https://www.gamer.com.tw/index2.php";
    
    /// <summary>
    /// 進入巴哈姆特連結 <c>https://www.gamer.com.tw/index2.php?ad=N</c>。
    /// </summary>
    private readonly By _homePageLinkBy = By.CssSelector("div.sky > a");
    
    private readonly IWebDriver _driver;

    public SpecialEventPage(IWebDriver driver)
    {
        _driver = driver;
    }
    
    public void ClickGoToHomePage()
    {
        var goToHomePageButton = _driver.FindElement(_homePageLinkBy);
        goToHomePageButton.Click();
    }
}
