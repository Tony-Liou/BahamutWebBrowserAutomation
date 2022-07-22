using OpenQA.Selenium;

namespace WebBrowserAutomation.Pages;

// TODO: This page is for Home page if there is an event page before entering the home page.
class SpecialEventPage
{
    private readonly WebDriver _driver;

    public SpecialEventPage(WebDriver driver)
    {
        _driver = driver;
    }
}
