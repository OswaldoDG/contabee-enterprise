using System.Net.Http.Headers;
using System.Net.Http;

public class TokenHandler : DelegatingHandler
{
    private readonly AuthService _authService;

    public TokenHandler(AuthService authService)
    {
        _authService = authService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (await _authService.isTokenExpired())
        {
            await _authService.refreshToken();
        }

        var token = await _authService.GetAccessToken();
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            System.Console.WriteLine(token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}