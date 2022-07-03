namespace WebBrowserAutomation;

public class Bahamut : BaseHttpClient
{
    public const string BaseUrl = "https://www.gamer.com.tw";
    private static readonly object locker = new();

    /// <summary>
    /// A static <see cref="HttpClient"/>. Do not <see cref="HttpClient.Dispose(bool)"/> it.
    /// </summary>
    private static HttpClient Client
    {
        get
        {
            if (_httpClient == null)
            {
                lock (locker)
                {
                    if (_httpClient == null)
                    {
                        _httpClient = new HttpClient
                        {
                            BaseAddress = new Uri(BaseUrl)
                        };
                    }
                }
            }
            return _httpClient;
        }
    }

    /// <summary>
    /// Test whether the website is running.
    /// </summary>
    /// <returns><c>true</c> if HTTP status code of resposne is 200ish; otherwise, <c>false</c>.</returns>
    public static async Task<bool> IsOperationalAsync()
    {
        var resp = await Client.GetAsync(string.Empty);
        return resp.IsSuccessStatusCode;
    }
}

public abstract class BaseHttpClient
{
    private protected static HttpClient? _httpClient;
}

