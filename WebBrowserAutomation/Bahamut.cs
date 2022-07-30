using System.Net;
using System.Net.Http.Json;
using Serilog;
using WebBrowserAutomation.Pages;

namespace WebBrowserAutomation;

public static class Bahamut
{
    /// <summary>
    /// DO NOT modify this field directly. Use <see cref="Client"/> instead.
    /// </summary>
    private static HttpClient? _client;

    /// <summary>
    /// DO NOT modify this field directly. Use <see cref="ClientHandler"/> instead.
    /// </summary>
    private static HttpClientHandler? _clientHandler;

    /// <summary>
    /// Do not <see cref="HttpClient.Dispose(bool)"/> it.
    /// </summary>
    private static HttpClient Client
    {
        get
        {
            return _client ??= new HttpClient { BaseAddress = new Uri(HomePage.Url) };
        }
    }

    /// <summary>
    /// Do not <see cref="HttpClientHandler.Dispose(bool)"/> it.
    /// </summary>
    private static HttpClientHandler ClientHandler
    {
        get
        {
            return _clientHandler ??= new HttpClientHandler();
        }
    }

    /// <summary>
    /// Send an HTTP request to test whether the website is running.
    /// </summary>
    /// <returns><c>true</c> if HTTP status code of response is 200ish; otherwise, <c>false</c>.</returns>
    public static async Task<bool> IsOperationalAsync()
    {
        var resp = await Client.GetAsync(string.Empty);
        return resp.IsSuccessStatusCode;
    }

    /// <summary>
    /// Check whether the user owning this <paramref name="cookieCollection"/> is signed in today.
    /// </summary>
    /// <param name="cookieCollection">All cookies in the Bahamut domain.</param>
    /// <returns><c>true</c> if today is signed in; otherwise, <c>false</c>.</returns>
    /// <remarks>This method create a new <see cref="HttpClient"/> instance every time you call it. Don't use it too much.</remarks>
    public static async Task<bool> IsSignedInAsync(IEnumerable<Cookie> cookieCollection)
    {
        CookieContainer cookies = new();
        foreach (var cookie in cookieCollection)
        {
            Log.Verbose("Cookie: {@Cookie}", cookie);
            cookies.Add(cookie);
        }

        ClientHandler.CookieContainer = cookies;
        FormUrlEncodedContent httpContent = new(new[] { new KeyValuePair<string, string>("action", "2") });
        using HttpClient client = new(ClientHandler, false) { BaseAddress = new Uri(HomePage.Url) };
        var resp = await client.PostAsync("/ajax/signin.php", httpContent);
        var body = await resp.Content.ReadFromJsonAsync<SignIn>();

        return body!.Data.Signin == 1;
    }

    record SignIn(Data Data);

    readonly record struct Data(int Days, int FinishedAd, int PrjSigninDays, int Signin);
}
