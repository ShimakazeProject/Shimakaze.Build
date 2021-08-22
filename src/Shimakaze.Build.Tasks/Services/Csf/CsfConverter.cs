using Shimakaze.Build.Tasks.Extensions;
using Shimakaze.Models.Csf;

using Shimakaze.Tools.Csf.Serialization.Csf;

namespace Shimakaze.Build.Tasks.Services.Csf;


internal static class CsfConverter
{
    public static string[] GetCsfLabels(Stream stream, Dictionary<string, CsfLabel> labels)
    {
        List<string> warnings = new();
        CsfStructSerializer serializer = new();
        var csf = serializer.DeserializeAsync(stream, new byte[4096]).WaitSync();
        csf.Datas.Each(x =>
        {
            var name = x.Label;
            // Duplicate check
            if (labels.TryGetValue(name.ToUpper(), out _))
                warnings.Add($"Duplicate label: {name}");

            labels[name.ToUpper()] = x;
        });
        return warnings.ToArray();
    }
}