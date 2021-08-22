using System.Xml;

using Shimakaze.Build.Tasks.Extensions;
using Shimakaze.Models.Csf;

namespace Shimakaze.Build.Tasks.Services.I18n;

public static class Generator
{
    public static XmlDocument GeneratI18nDocument(CsfLabel[] labels)
    {
        XmlDocument doc = new();
        // <I18n>
        XmlElement root = doc.CreateElement("I18n");

        labels.Select(label => CreateLabelElement(doc, label)).Each(x => root.AppendChild(x));

        doc.AppendChild(root);
        // </I18n>
        return doc;
    }

    public static void AppendI18nDocument(CsfLabel[] labels, XmlDocument doc)
    {
        var root = doc.DocumentElement;

        if (root is null)
            throw new Exception("Invalid I18n document");

        Dictionary<string, XmlElement> elements = new();
        root.ChildNodes.Each((XmlElement element) =>
        {
            var name = element.Name switch
            {
                not "Label" => throw new Exception("Invalid XML"),
                _ => element.GetAttribute("name")
            };
            elements.Add(name, element);
        });

        labels
            .Where(label => elements.ContainsKey(label.Label))
            .Where(label => elements[label.Label].ChildNodes.Count != label.Values.Length)
            .Each(label =>
            {
                var element = elements[label.Label];
                if (label.Values.Length > element.ChildNodes.Count)
                {
                    for (int i = element.ChildNodes.Count; i < label.Values.Length; i++)
                        element.AppendChild(CreateValueElement(doc, label.Values[i]));
                }
                else
                {
                    for (int i = label.Values.Length; i < element.ChildNodes.Count; i++)
                        element.RemoveChild(element.ChildNodes[i]!);
                }
            });

        labels
            .Where(label => !elements.ContainsKey(label.Label))
            .Select(label => CreateLabelElement(doc, label))
            .Each(x => root.AppendChild(x));
    }

    public static CsfLabel[] GetAllTargetLabels(XmlDocument doc)
    {
        var root = doc.DocumentElement;

        if (root is null)
            throw new Exception("Invalid I18n document");

        List<CsfLabel> labels = new();

        root.ChildNodes.Each((XmlElement element) =>
        {
            List<CsfValue> values = new();
            element.ChildNodes.Each((XmlElement value) =>
            {
                var target = value["Target"];
                if (target is null)
                    throw new Exception("Invalid XML");

                string? extra = default;
                if (target.HasAttribute("extra"))
                    extra = target.GetAttribute("extra");

                values.Add(string.IsNullOrEmpty(extra) ? new CsfValue(target.InnerText) : new CsfExtraValue(target.InnerText, extra));
            });

            labels.Add(new()
            {
                Label = element.GetAttribute("name"),
                Values = values.ToArray(),
            });
        });

        return labels.ToArray();
    }


    private static XmlElement CreateValueElement(XmlDocument doc, CsfValue value)
    {
        // <Value>
        XmlElement valueElement = doc.CreateElement("Value");
        valueElement.SetAttribute("status", "Untranslated");

        if (value is CsfExtraValue extra)
            valueElement.SetAttribute("extra", extra.Extra);

        // <Source>
        XmlElement source = doc.CreateElement("Source");
        source.InnerText = value.Value;
        valueElement.AppendChild(source);
        // </Source>

        // <Target>
        XmlElement target = doc.CreateElement("Target");
        target.InnerText = string.Empty;
        valueElement.AppendChild(target);
        // </Target>

        return valueElement;
        // </Value>
    }

    private static XmlElement CreateLabelElement(XmlDocument doc, CsfLabel label)
    {
        // <Label name="label">
        XmlElement element = doc.CreateElement("Label");
        element.SetAttribute("name", label.Label);

        label.Values.Each(v => element.AppendChild(CreateValueElement(doc, v)));

        return element;
        // </Label>
    }
}