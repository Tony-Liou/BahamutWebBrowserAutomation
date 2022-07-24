using Microsoft.Extensions.Configuration;

namespace WebBrowserAutomation.Configurations;

public class ChromeOptions
{
    /// <summary>
    /// A list of arguments appended to the Chrome command line as a string array.
    /// </summary>
    [ConfigurationKeyName("Args")]
    public IEnumerable<string>? Arguments { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the command prompt window of the service should be hidden.
    /// </summary>
    public bool HideCommandPromptWindow { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the initial diagnostic information is suppressed
    /// when starting the driver server executable. Defaults to <see langword="false"/>, meaning
    /// diagnostic information should be shown by the driver server executable.
    /// </summary>
    public bool SuppressInitialDiagnosticInformation { get; set; }
}
