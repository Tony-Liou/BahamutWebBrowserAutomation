using OpenQA.Selenium;

namespace WebBrowserAutomation.Pages.Utils;

public static class PageUtils
{
    public static bool CheckSamePage(IWebDriver driver, string uri)
    {
        return CheckSamePage(driver, new Uri(uri));
    }
    
    public static bool CheckSamePage(IWebDriver driver, Uri uri)
    {
        if (uri.Equals(driver.Url))
        {
            return true;
        }

        driver.Navigate().GoToUrl(uri);
        return false;
    }
}
