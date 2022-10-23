using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;

namespace WebBrowserAutomation.Configurations;

public class SeleniumOptions
{
    public const string ConfigurationSectionName = "SeleniumConfigs";
    
    /// <summary>
    /// Implicit wait time in seconds.
    /// See <see href="https://www.selenium.dev/documentation/webdriver/waits/#implicit-wait">Implicit wait</see> for more information.
    /// </summary>
    [ConfigurationKeyName("ImplicitWaitTimeInSeconds")]
    [Range(0, Single.MaxValue)]
    public float ImplicitWaitInSec { get; init; }

    /// <summary>
    /// Explicit wait time in seconds.
    /// See <see href="https://www.selenium.dev/documentation/webdriver/waits/#explicit-wait">Explicit wait</see> for more information.
    /// </summary>
    [ConfigurationKeyName("ExplicitWaitTimeInSeconds")]
    [Range(0, Single.MaxValue)]
    public float ExplicitWaitInSec { get; init; } = 3;

    /// <summary>
    /// Polling interval in milliseconds.
    /// See <see href="https://www.selenium.dev/documentation/webdriver/waits/#fluentwait">Fluent wait</see> for more information.
    /// </summary>
    [ConfigurationKeyName("PollingIntervalInMilliseconds")]
    [Range(0, int.MaxValue)]
    public float PollingIntervalInMs { get; init; } = 500;
}
