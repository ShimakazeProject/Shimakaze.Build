
using System.Text;
using System.Text.Json;

using Shimakaze.Build.Tasks.Extensions;
using Shimakaze.Models.Csf;

namespace Shimakaze.Build.Tasks.Services.Csf;

internal static class JsonConverter
{
    /// <summary>
    /// Parse Csf JSON to CsfModel
    /// </summary>
    /// <param name="labels">Reference result</param>
    public static string[] GetCsfLabels(Stream stream, Dictionary<string, CsfLabel> labels)
    {
        using var json = JsonDocument.Parse(stream);

        if (json is null)
            throw new InvalidOperationException("Failed to parse JSON.");

        if (!json.RootElement.TryGetProperty("protocol", out var j_protocol))
            throw new JsonException("Protocol not found.");


        int protocol = j_protocol.GetInt32();
        return protocol switch
        {
            1 => V1(json.RootElement, labels),
            2 => V2(json.RootElement, labels),
            _ => throw new JsonException($"Unsupported protocol: {protocol}"),
        };

    }

    private static string[] V1(JsonElement root, Dictionary<string, CsfLabel> labels)
    {
        List<string> warnings = new();

        // Try to get labels
        if (!root.TryGetProperty("data", out var j_data))
            throw new JsonException("Data not found.");

        j_data.EnumerateArray().Each(x =>
        {
            if (!x.TryGetProperty("label", out var j_label))
                throw new JsonException("Label not found.");

            if (j_label.ValueKind is not JsonValueKind.String)
                throw new JsonException("Label is not a string.");

            string name = j_label.GetString()!;

            // Duplicate check
            if (labels.TryGetValue(name.ToUpper(), out _))
                warnings.Add($"Duplicate label: {name}");

            labels[name.ToUpper()] = new CsfLabel
            {
                Label = name,
                Values = ParseCsfValuesObject(x),
            };
        });
        return warnings.ToArray();
    }

    private static string[] V2(JsonElement root, Dictionary<string, CsfLabel> labels)
    {
        List<string> warnings = new();

        // Try to get labels
        if (!root.TryGetProperty("data", out var j_data))
            throw new JsonException("Data not found.");

        j_data.EnumerateObject().Each(x =>
        {
            JsonElement j_value = x.Value;

            // Duplicate check
            if (labels.TryGetValue(x.Name.ToUpper(), out _))
                warnings.Add($"Duplicate label: {x.Name}");


            labels[x.Name.ToUpper()] = new CsfLabel
            {
                Label = x.Name,
                Values = j_value.ValueKind switch
                {
                    // Multi-line string
                    JsonValueKind.Array => new[] { new CsfValue(GetMultiLineString(j_value)) },
                    // Single-line string
                    JsonValueKind.String => new[] { new CsfValue(j_value.GetString()!) },
                    // Is a complex string structure
                    JsonValueKind.Object => ParseCsfValuesObject(j_value),
                    _ => throw new JsonException("Invalid value type."),
                }
            };
        });
        return warnings.ToArray();
    }

    private static string GetMultiLineString(JsonElement json)
    {
        StringBuilder sb = new();
        json.EnumerateArray()
            .Where(x => x.ValueKind is JsonValueKind.String)
            .Select(x => x.GetString())
            .Each(x => sb.AppendLine(x));
        return sb.ToString();
    }

    private static CsfValue GetCsfValueObject(JsonElement json)
    {
        string? value;
        string? extra = default;

        value = json.TryGetProperty("value", out var j_value)
            ? j_value.ValueKind switch
            {
                // Single-line string
                JsonValueKind.String => j_value.GetString(),
                // Multi-line string
                JsonValueKind.Array => GetMultiLineString(j_value),
                _ => throw new JsonException("Invalid value type."),
            }
            // If the value cannot be obtained, an exception is thrown.
            : throw new JsonException("Value not found.");

        if (json.TryGetProperty("extra", out var j_extra))
        {
            extra = j_extra.ValueKind switch
            {
                JsonValueKind.String => j_extra.GetString(),
                _ => throw new JsonException("Invalid value type."),
            };
        }

        return value switch
        {
            null => throw new JsonException("Value not found."),
            not null => extra switch
            {
                null => new CsfValue(value),
                not null => new CsfExtraValue(value, extra),
            }
        };
    }

    private static CsfValue[] ParseCsfValuesObject(JsonElement json)
    {
        return json.TryGetProperty("values", out var j_values)
            // is a Multiple String
            ? j_values.ValueKind switch
            {
                JsonValueKind.Array => j_values.EnumerateArray().Select(x => x.ValueKind switch
                {
                    // Single-line string
                    JsonValueKind.String => new CsfValue(x.GetString()!),
                    // Multi-line string
                    JsonValueKind.Array => new CsfValue(GetMultiLineString(x)),
                    // Is a complex string structure
                    JsonValueKind.Object => GetCsfValueObject(x),
                    _ => throw new JsonException("Invalid value type."),
                }).ToArray(),
                _ => throw new JsonException("Invalid value type."),
            }
            // Is a Single String
            : (new[] { GetCsfValueObject(json) });
    }


}
