using System.Text.Json.Serialization;

namespace Shimakaze.MSBuild.Model;

internal class JsonProtocol
{
    [JsonPropertyName("protocol")]
    public int? Protocol { get; set; }
}