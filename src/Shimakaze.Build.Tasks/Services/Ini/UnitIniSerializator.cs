

using Shimakaze.Models.Ini;

using IniDocument = Shimakaze.Build.Tasks.Model.UnitIniDocument;
using IniSection = Shimakaze.Build.Tasks.Model.UnitSection;


namespace Shimakaze.Build.Tasks.Services.Ini;

internal static class UnitIniSerializer
{
    public static IniDocument Deserialize(TextReader tr)
    {
        IniDocument iniDocument = new();
        IniSection iniSection = iniDocument.Default;
        while (tr.Peek() > 0)
        {
            string? text = tr.ReadLine();
            if (!string.IsNullOrWhiteSpace(text))
            {
                if (text.StartsWith("["))
                {
                    string section = text.Split('[').Last().Split(']')
                        .First();
                    iniSection = iniDocument.Add(section);
                    var tmp = text.Split(":");
                    if (tmp.Length > 1)
                    {
                        var tmp2 = tmp[1].Split(';', '#').First();
                        var arr = tmp2.Split(',');
                        iniSection.Types = arr.Select(GetType).ToArray();
                    }
                }
                else
                {
                    string[] source = text.Split('=');
                    iniSection.Add(source.First().Trim(), source.Last().Split(';', '#').First().Trim());
                }
            }
        }
        return iniDocument;
    }

    private static (string Type, string Key) GetType(string s)
    {
        var arr = s.Split('(');
        string type = arr.First().Trim();
        string key = arr.Last().Split(')').First().Trim();
        return (type, key);
    }

    public static void Serialize(IniDocument ini, TextWriter tw)
    {
        foreach (KeyValuePair<string, IniValue> item in ini.Default)
        {
            tw.WriteLine(item.Key + "=" + item.Value);
        }
        tw.WriteLine();
        foreach (IniSection item2 in ini)
        {
            tw.WriteLine("[" + item2.Name + "]");
            foreach (KeyValuePair<string, IniValue> item3 in item2)
            {
                tw.WriteLine(item3.Key + "=" + item3.Value);
            }
            tw.WriteLine();
        }
    }
}
