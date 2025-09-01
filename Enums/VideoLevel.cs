using System.Text.Json.Serialization;

namespace InvestNaijaAuth.Enums
{
    public enum VideoLevel
    {
        [JsonPropertyName("Beginner")]
        Beginner ,
        [JsonPropertyName("Intermediate")]
        Advanced ,
        [JsonPropertyName("Advanced")]
        Expert
    }
}
