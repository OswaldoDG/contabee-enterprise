using Blazored.SessionStorage;
using staterkit.Models;

public class AuthService
{
    private readonly HttpClient _http;
    private readonly ISessionStorageService _sessionStorageService;
    private const string _authToken = "access_token";
    private const string _refreshToken = "refresh_token";
    private const string _sessionToken = "expiry";

    public event Action? OnRefreshStateChanged;
    public bool IsRefreshing { get; private set; }


    public AuthService(HttpClient http, ISessionStorageService sessionStorageService)
    {
        _http = http;
        _sessionStorageService = sessionStorageService;
    }

    public async Task saveToken(LoginResponse response)
    {
        //se establece 2 minutos de refreshToken para pruebas.
        var manualExpire = DateTime.Now.AddMinutes(2);

        await _sessionStorageService.SetItemAsync(_authToken, response.access_token);
        await _sessionStorageService.SetItemAsync(_refreshToken, response.refresh_token);
        // await _sessionStorageService.SetItemAsync(_sessionToken, response.expires_in);
        await _sessionStorageService.SetItemAsync(_sessionToken, manualExpire);
        var token = await _sessionStorageService.GetItemAsStringAsync(_authToken);
    }

    public async Task<string?> GetAccessToken() => await _sessionStorageService.GetItemAsync<string>(_authToken);

    private void NotifyStateChanged() => OnRefreshStateChanged?.Invoke();

    public async Task<string?> refreshToken()
    {
        var refreshToken = await _sessionStorageService.GetItemAsync<string>(_refreshToken);
        if (string.IsNullOrWhiteSpace(refreshToken)) return null;

        var dict = new Dictionary<string, string>
        {
            {"client_id","contabee-password"},
            {"scope","offline_access"},
            {"dispositivoid","00000000-0000-0000-0000-000000000012"},
            {"refresh_token",refreshToken},
            {"grant_type","refresh_token"},
        };

        var content = new FormUrlEncodedContent(dict);
        var response = await _http.PostAsync("/api/identity/connect/token", content);

        IsRefreshing = true;
        NotifyStateChanged();
        try
        {
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (result != null)
                {
                    await saveToken(result);
                    return result.access_token;
                }
            }
        }
        finally
        {
            IsRefreshing = false;
            NotifyStateChanged();
        }


        await Logout();
        return null;

    }

    public async Task Logout()
    {
        await _sessionStorageService.ClearAsync();
    }

    public async Task<bool> isTokenExpired()
    {
        var expireTime = await _sessionStorageService.GetItemAsync<DateTime>("expiry");
        if (expireTime is DateTime date)
        {
            return date <= DateTime.Now.AddMinutes(1);
        }
        return true;
    }

    public async Task checkAndRefreshIfNeeded()
    {
        if (await isTokenExpired())
        {
            await refreshToken();
        }
    }

}