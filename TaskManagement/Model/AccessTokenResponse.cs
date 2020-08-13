using Newtonsoft.Json;

namespace TaskManagement.Model
{
    public class AccessTokenResponse
    {
        /// <summary>
        /// Gets or sets tokenType
        /// </summary>
        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        /// <summary>
        /// Gets or sets ExpiresIn
        /// </summary>
        [JsonProperty("expires_in")]
        public string ExpiresIn { get; set; }

        /// <summary>
        /// Gets or sets ExtExpiresIn
        /// </summary>
        [JsonProperty("ext_expires_in")]
        public string ExtExpiresIn { get; set; }

        /// <summary>
        /// Gets or sets AccessToken
        /// </summary>
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
    }
}
