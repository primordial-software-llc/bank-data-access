using Newtonsoft.Json;

namespace PropertyRentalManagement.QuickBooksOnline
{
    public class QuickBooksOnlineBearerToken
	{
        [JsonProperty("x_refresh_token_expires_in")]
        public long XRefreshTokenExpiresIn { get; set; }
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        [JsonProperty("token_type")]
        public string TokenType { get; set; }
        [JsonProperty("expires_in")]
        public long ExpiresIn { get; set; }
    }
}
