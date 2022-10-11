using WebBrowserAutomation.Configurations;

namespace WebBrowserAutomation;

/// <summary>
/// Used to store the configuration for this application.
/// </summary>
public static class Global
{
    public static SeleniumOptions SeleniumOptions { get; set; } = new();
}

