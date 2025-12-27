namespace staterkit.Models
{
    public class LoginRequest
    {
        public string username { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;

    }

    public class LoginResponse
    {
        public string access_token { get; set; } = string.Empty;
        public string token_type { get; set; } = string.Empty;
        public int expires_in { get; set; } = 0;
        public string refresh_token { get; set; } = string.Empty;
    }
}