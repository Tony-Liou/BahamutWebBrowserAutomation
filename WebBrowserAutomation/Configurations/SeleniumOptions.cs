namespace WebBrowserAutomation.Configurations;

public readonly struct SeleniumOptions
{
    public SeleniumOptions() { }

    /// <summary>
    /// Implicit wait time in seconds.
    /// See <see href="https://www.selenium.dev/documentation/webdriver/waits/#implicit-wait">Implicit wait</see> for more information.
    /// </summary>
    public float ImplicitWaitInSec { get; init; } = 0;

    /// <summary>
    /// Explicit wait time in seconds.
    /// See <see href="https://www.selenium.dev/documentation/webdriver/waits/#explicit-wait">Explicit wait</see> for more information.
    /// </summary>
    public float ExplicitWaitInSec { get; init; } = 3;

    /// <summary>
    /// Polling interval in milliseconds.
    /// See <see href="https://www.selenium.dev/documentation/webdriver/waits/#fluentwait">Fluent wait</see> for more information.
    /// </summary>
    public float PollingIntervalInMs { get; init; } = 500;
}
