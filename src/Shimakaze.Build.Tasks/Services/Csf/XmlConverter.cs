using System.Xml;

using Shimakaze.Build.Tasks.Extensions;
using Shimakaze.Models.Csf;

namespace Shimakaze.Build.Tasks.Services.Csf;

internal static class XmlConverter
{
    /// <summary>
    /// Parse Csf JSON to CsfModel
    /// </summary>
    /// <param name="labels">Reference result</param>
    public static string[] GetCsfLabels(Stream stream, Dictionary<string, CsfLabel> labels)
    {
        XmlDocument xmldoc = new();
        xmldoc.Load(stream);

        var root = xmldoc.DocumentElement;
        var s_protocol = root?.GetAttribute("protocol");
        if (!int.TryParse(s_protocol, out int protocol))
            throw new XmlException("Invalid protocol");

        return protocol switch
        {
            1 => V1(root!, labels),
            _ => throw new XmlException($"Unsupported protocol: {protocol}"),
        };

    }

    private static string[] V1(XmlElement root, Dictionary<string, CsfLabel> labels)
    {
        List<string> warnings = new();

        root.ChildNodes.Each((XmlElement x) =>
        {
            var name = x.GetAttribute("name");

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

    private static CsfValue GetCsfValueObject(XmlElement xml)
    {
        string value;
        string? extra = default;

        if (xml.HasAttribute("extra"))
            extra = xml.GetAttribute("extra");

        value = xml.InnerText;


        return extra switch
        {
            null => new CsfValue(value),
            not null => new CsfExtraValue(value, extra),
        };
    }

    private static CsfValue[] ParseCsfValuesObject(XmlElement xml)
    {
        var x_value = xml.FirstChild;
        return x_value switch
        {
            null => new[] { new CsfValue(string.Empty) },
            not null => x_value.NodeType switch
            {
                XmlNodeType.Text => new[] { GetCsfValueObject(xml) },
                XmlNodeType.Element => x_value is not XmlElement values
                    ? throw new XmlException("Invalid value")
                    : values.Name switch
                    {
                        "Values" => values.ChildNodes.Each((XmlElement x) => GetCsfValueObject(x)).ToArray(),
                        "Value" => new[] { GetCsfValueObject(values) },
                        _ => throw new XmlException($"Unsupported value type: {values.Name}"),
                    },
                _ => throw new XmlException("Invalid value"),
            }
        };
    }
}
